# evm4-analog-framework

Purpose-built target for AppSec tool evaluation. **Deliberately vulnerable. Do not deploy.**

Nothing public tests the pattern EVM4 actually uses: a shared framework distributed as
**private NuGet packages**, where the interesting third-party dependencies are transitive
from a service's point of view. This repo reproduces that in miniature.

## Shape

```
DemoFramework  (class library)  -->  Newtonsoft.Json 12.0.3   CVE-2024-21907
                                -->  System.Net.Http   4.3.0   CVE-2017-0247 et al

DemoService    (web API)        -->  DemoFramework            [its ONLY dependency]
```

`DemoService` has **one** dependency and never references Newtonsoft.Json directly.
The CVEs arrive transitively, exactly as EVM4 services inherit the framework's 75 packages.

## The taint path

```
EvidenceController.Import([FromBody] payload)     <- untrusted input, DemoService
  -> DataProcessor.Parse<T>(payload)              <- DemoFramework
     -> JsonConvert.DeserializeObject(..., TypeNameHandling.All)   <- vulnerable sink
```

Crosses a project boundary in stages 1 and 2, and a **compiled package boundary** in stage 3.

## Four stages

| Stage | Setup | Question it answers |
|---|---|---|
| **1** | As committed: `ProjectReference`, **no lockfile** | Does the tool infer the transitive tree from `.csproj` alone? |
| **2** | Add `Directory.Build.props` with `RestorePackagesWithLockFile`, restore, commit `packages.lock.json` | **Does the lockfile fix transitive resolution?** |
| **3** | Run `./pack-to-local-feed.sh`, switch `DemoService` to `PackageReference` | Now it matches EVM4. Does anything still see Newtonsoft? Is reachability still possible? |
| **4** | Scan `DemoFramework` as its own repo | Does reachability return when the dependency is **direct**? |

Stage 2 tests the core claim in `sca-discoverability-build-changes.md`.
Stage 4 tests whether onboarding `crossbones-framework` actually recovers visibility.

## Scoring

For each tool at each stage, record:

- Is `Newtonsoft.Json 12.0.3` reported at all?
- Is `System.Net.Http 4.3.0` reported?
- Reachability verdict: **reachable / not reachable / unknown**
- At stage 3 especially: does the tool say *"not reachable"* (wrong, it cannot see inside
  the package) or *"unknown"* (honest)? **Unknown is the better answer.**

## Ground truth

```
dotnet restore
dotnet list package --vulnerable --include-transitive
```
