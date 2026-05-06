using IPS;
using IPS.Jobs;
using Xunit;

namespace IPS.Tests;

// =============================================================================
// Simple unit tests for the Industrial Processing System.
// Each test follows the same pattern:
//   1. Set up an input
//   2. Call a method
//   3. Check the result with Assert
// =============================================================================

public class JobTests
{
    [Fact]
    public void Job_Constructor_SetsAllFields()
    {
        // Make a Job and check that all four fields are filled in.
        var job = new Job(JobType.Prime, "numbers:100,threads:2", 5);

        Assert.Equal(JobType.Prime, job.Type);
        Assert.Equal("numbers:100,threads:2", job.Payload);
        Assert.Equal(5, job.Priority);
        Assert.NotEqual(Guid.Empty, job.Id); // a real Guid was assigned
    }

    [Fact]
    public async Task JobHandle_Complete_GivesBackTheResult()
    {
        // Make a JobHandle, complete it with the value 42, await, expect 42 back.
        var handle = new JobHandle(Guid.NewGuid());
        handle.Complete(42);

        int result = await handle.Result;

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task JobHandle_Abort_ThrowsJobAbortedException()
    {
        // Make a JobHandle, abort it, expect the await to throw.
        var handle = new JobHandle(Guid.NewGuid());
        handle.Abort();

        await Assert.ThrowsAsync<JobAbortedException>(async () => await handle.Result);
    }

    [Fact]
    public void JobAbortedException_RemembersTheJobId()
    {
        var id = Guid.NewGuid();
        var ex = new JobAbortedException(id);

        Assert.Equal(id, ex.JobId);
    }
}

public class PrimeJobTests
{
    [Fact]
    public void PrimeJob_PrimesUpToTen_AreFour()
    {
        // Primes <= 10 are 2, 3, 5, 7. So the answer must be 4.
        int result = PrimeJob.Execute("numbers:10,threads:1", CancellationToken.None);

        Assert.Equal(4, result);
    }

    [Fact]
    public void PrimeJob_PrimesUpToHundred_AreTwentyFive()
    {
        // Standard reference: there are 25 primes <= 100.
        int result = PrimeJob.Execute("numbers:100,threads:2", CancellationToken.None);

        Assert.Equal(25, result);
    }

    [Fact]
    public void PrimeJob_HandlesUnderscoresInNumbers()
    {
        // "10_000" should be parsed as 10000. There are 1229 primes <= 10000.
        int result = PrimeJob.Execute("numbers:10_000,threads:2", CancellationToken.None);

        Assert.Equal(1229, result);
    }

    [Fact]
    public void PrimeJob_ClampsThreadCountToMaximumEight()
    {
        // The spec requires threads to be clamped to [1, 8].
        // Asking for 100 threads should clamp to 8.
        var (_, threads) = PrimeJob.ParsePayload("numbers:100,threads:100");

        Assert.Equal(8, threads);
    }
}

public class IOJobTests
{
    [Fact]
    public void IOJob_ReturnsNumberBetweenZeroAndHundred()
    {
        // IO returns a random number from 0 to 100.
        int result = IOJob.Execute("delay:10", CancellationToken.None);

        Assert.InRange(result, 0, 100);
    }

    [Fact]
    public void IOJob_ParsesDelayWithUnderscores()
    {
        // "1_000" should be parsed as 1000.
        int delay = IOJob.ParsePayload("delay:1_000");

        Assert.Equal(1000, delay);
    }
}

public class SystemConfigTests : IDisposable
{
    private readonly string _xmlPath;

    public SystemConfigTests()
    {
        _xmlPath = Path.Combine(Path.GetTempPath(), $"cfg-{Guid.NewGuid():N}.xml");

        File.WriteAllText(_xmlPath,
            @"<?xml version=""1.0"" encoding=""utf-8""?>
            <SystemConfig>
              <WorkerCount>3</WorkerCount>
              <MaxQueueSize>50</MaxQueueSize>
              <Jobs>
                <Job Type=""Prime"" Payload=""numbers:100,threads:2"" Priority=""1""/>
                <Job Type=""IO"" Payload=""delay:500"" Priority=""2""/>
              </Jobs>
            </SystemConfig>");
    }

    public void Dispose()
    {
        // Clean up the temp file after the test runs.
        if (File.Exists(_xmlPath)) File.Delete(_xmlPath);
    }

    [Fact]
    public void Config_LoadsWorkerCountAndQueueSize()
    {
        var config = SystemConfig.Load(_xmlPath);

        Assert.Equal(3, config.WorkerCount);
        Assert.Equal(50, config.MaxQueueSize);
    }

    [Fact]
    public void Config_LoadsAllInitialJobs()
    {
        var config = SystemConfig.Load(_xmlPath);

        Assert.Equal(2, config.InitialJobs.Count);
    }
}

public class ProcessingSystemTests
{
    private static SystemConfig SimpleConfig() => new()
    {
        WorkerCount = 2,
        MaxQueueSize = 100,
        InitialJobs = Array.Empty<Job>()
    };

    [Fact]
    public async Task Submit_PrimeJob_WorksEndToEnd()
    {
        // Submit a Prime job to the system, wait for it to finish, check the result.
        // This exercises submit, the queue, the worker thread, and JobHandle completion.
        using var system = new ProcessingSystem(SimpleConfig());

        var job = new Job(JobType.Prime, "numbers:100,threads:2", 1);
        var handle = system.Submit(job);

        Assert.NotNull(handle);
        int result = await handle!.Result.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(25, result);
    }

    [Fact]
    public async Task Submit_IOJob_ReturnsNumberInRange()
    {
        // Submit an IO job and check that the result is between 0 and 100 (per spec).
        using var system = new ProcessingSystem(SimpleConfig());

        var handle = system.Submit(new Job(JobType.IO, "delay:30", 1));

        Assert.NotNull(handle);
        int result = await handle!.Result.WaitAsync(TimeSpan.FromSeconds(3));
        Assert.InRange(result, 0, 100);
    }

    [Fact]
    public void Submit_SameIdTwice_ReturnsSameHandle()
    {
        // The spec says: same Job (same Id) must not be executed multiple times.
        // Submitting the same Id twice should give back the same JobHandle object.
        using var system = new ProcessingSystem(SimpleConfig());

        var id = Guid.NewGuid();
        var job1 = new Job(id, JobType.IO, "delay:50", 1);
        var job2 = new Job(id, JobType.IO, "delay:50", 1);

        var handle1 = system.Submit(job1);
        var handle2 = system.Submit(job2);

        Assert.Same(handle1, handle2);
    }

    [Fact]
    public void GenerateReport_WritesAnXmlFile()
    {
        // Calling GenerateReport should produce a real XML file on disk.
        using var system = new ProcessingSystem(SimpleConfig());

        string path = system.GenerateReport();

        Assert.True(File.Exists(path));
        Assert.EndsWith(".xml", path);
    }
}
