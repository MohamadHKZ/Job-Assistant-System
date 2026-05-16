using API.Data;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace API.Services;

class JobsService(AppDbContext _dbContext) : IJobsService
{
    private sealed class VectorRankedJobIdRow
    {
        public Guid Id { get; set; }
        public float Score { get; set; }
    }

    public async Task<IEnumerable<EmbeddedJobPost>> GetAllEmbeddedJobPostsAsync()
    {
        return await _dbContext.EmbeddedJobPosts.Include(jp => jp.NormalizedJobPost).ThenInclude(njp => njp.JobPost).ToListAsync();
    }

    public async Task<IEnumerable<EmbeddedJobPost>> GetJobsBySimilarTitleAsync(Vector profileJobTitleVector, int topN = 100)
    {
        return await _dbContext.EmbeddedJobPosts
            .Include(jp => jp.NormalizedJobPost)
                .ThenInclude(njp => njp.JobPost)
            .OrderBy(jp => jp.EmbeddedJobTitle.CosineDistance(profileJobTitleVector))
            .Take(topN)
            .ToListAsync();
    }

    public async Task<(List<EmbeddedJobPost> Items, bool HasNextPage, float? NextCursorScore, Guid? NextCursorId)> GetPagedJobsAsync(
        Vector profileJobTitleVector,
        int pageSize,
        float? cursorScore,
        Guid? cursorId,
        CancellationToken cancellationToken = default)
    {
        var fetchCount = pageSize + 1;
        var idScorePairs = await QueryVectorRankedJobIdsAsync(
            profileJobTitleVector,
            fetchCount,
            cursorScore,
            cursorId,
            cancellationToken).ConfigureAwait(false);

        var hasNext = idScorePairs.Count > pageSize;
        var pagePairs = idScorePairs.Take(pageSize).ToList();

        float? nextCursorScore = null;
        Guid? nextCursorId = null;
        if (hasNext && pagePairs.Count > 0)
        {
            var last = pagePairs[^1];
            nextCursorScore = last.Score;
            nextCursorId = last.Id;
        }

        if (pagePairs.Count == 0)
            return ([], false, null, null);

        var ids = pagePairs.Select(p => p.Id).ToList();
        var posts = await _dbContext.EmbeddedJobPosts
            .Include(jp => jp.NormalizedJobPost)
                .ThenInclude(njp => njp!.JobPost)
            .Where(jp => ids.Contains(jp.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var order = pagePairs.Select(p => p.Id).ToList();
        var lookup = posts.ToDictionary(jp => jp.Id);
        var ordered = new List<EmbeddedJobPost>();
        foreach (var id in order)
        {
            if (lookup.TryGetValue(id, out var jp))
                ordered.Add(jp);
        }

        return (ordered, hasNext, nextCursorScore, nextCursorId);
    }

    public async Task<EmbeddedJobPost?> GetFullJobPostByIdAsync(Guid jobId)
    {
        return await _dbContext.EmbeddedJobPosts.Include(jp => jp.NormalizedJobPost).ThenInclude(njp => njp.JobPost)
            .FirstOrDefaultAsync(jp => jp.Id == jobId);
    }

    // Upper bound on how many ANN candidates the index scan retrieves before
    // score-threshold filtering. All rows with Score > 0.6 have a cosine distance
    // < 0.4, so they are always the nearest neighbours and will be included as
    // long as this limit exceeds the total number of qualifying rows.
    private const int CandidateLimit = 5_00;

    /// <summary>
    /// Vector title similarity keyset query. Score is ROUND((1 - cosine_distance)::numeric, 2) as real.
    /// </summary>
    private async Task<List<(Guid Id, float Score)>> QueryVectorRankedJobIdsAsync(
        Vector profileJobTitleVector,
        int take,
        float? cursorScore,
        Guid? cursorId,
        CancellationToken cancellationToken)
    {
        // MATERIALIZED forces the inner SELECT to run first using the vector index
        // (ORDER BY col <=> vec LIMIT n is the pattern pgvector HNSW/IVFFlat indexes
        // recognise). The outer query then applies the score threshold and keyset
        // cursor on the already-materialised rows without touching the index again.
        const string cte = """
            WITH nearest AS MATERIALIZED  (
                SELECT ep."Id",
                       ROUND((1 - (ep."EmbeddedJobTitle" <=> @profile_vec))::numeric, 2)::real AS "Score"
                FROM "EmbeddedJobPosts" ep
                ORDER BY ep."EmbeddedJobTitle" <=> @profile_vec ASC
                LIMIT @candidate_limit
            )
            SELECT "Id", "Score"
            FROM nearest
            WHERE "Score" > 0.6
            """;

        List<VectorRankedJobIdRow> rows;
        if (cursorScore is null || cursorId is null)
        {
            var sql = cte + """
                
                ORDER BY "Score" DESC, "Id" ASC
                LIMIT @take
                """;
            rows = await _dbContext.Database
                .SqlQueryRaw<VectorRankedJobIdRow>(
                    sql,
                    new NpgsqlParameter("profile_vec", profileJobTitleVector) { DataTypeName = "vector" },
                    new NpgsqlParameter("candidate_limit", CandidateLimit),
                    new NpgsqlParameter("take", take))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            var sql = cte + """
                  AND (
                      "Score" < @cursor_score
                      OR ("Score" = @cursor_score AND "Id" > @cursor_id)
                  )
                ORDER BY "Score" DESC, "Id" ASC
                LIMIT @take
                """;
            rows = await _dbContext.Database
                .SqlQueryRaw<VectorRankedJobIdRow>(
                    sql,
                    new NpgsqlParameter("profile_vec", profileJobTitleVector) { DataTypeName = "vector" },
                    new NpgsqlParameter("candidate_limit", CandidateLimit),
                    new NpgsqlParameter("take", take),
                    new NpgsqlParameter("cursor_score", cursorScore.Value),
                    new NpgsqlParameter("cursor_id", cursorId.Value))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return rows.ConvertAll(r => (r.Id, r.Score));
    }
}