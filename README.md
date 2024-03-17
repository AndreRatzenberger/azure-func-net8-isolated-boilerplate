
# Boilerplate Code for Durable Functions on .NET 8 Isolated and Apple Silicon

This repository emerges as a lighthouse amidst the fog of Azure's official documentation and tooling mishaps. If you've ever found yourself in the treacherous waters of setting up Azure Functions from the ground up, especially the new "That's how it should be done" way all about isolated processes, where official examples lead to dead ends and commands vanish like mirages, consider this your haven.

The journey through the isolated setup can feel akin to navigating a labyrinth designed by someone who forgot to include an exit. You're instructed to invoke commands and templates that, for all intents and purposes, seem to have been conjured from thin air. Not only your function are running in isolation. Your mind does too.

Suppose you miraculously navigate your project past the nonexistent NuGet packages and/or version mismatches to a compilable state. In that case, don't be surprised if a runtime bug promptly greets you, turning your triumph into turmoil. To add insult to injury, the conventional boilerplate code hardly scratches the surface of what durable functions can truly offer.

Why settle for boring boilerplate use cases? This project features an exhilarating race between various animals, a spectacle designed to showcase the fan-out/fan-in pattern in all its glory. This is not just code; it's a statement against the mundane, promising a sprinkle of excitement in the world of Hello Worlds!.

So save your sanity, grab this repo, and let's get down to business.

## Requirements

- **VS Code**: Because why torture yourself with anything less? Grab it [here](https://code.visualstudio.com/download).
- **Azure Functions Extension for VS Code**: Slap this onto VS Code from the [Marketplace](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions).
- [**Azurite**](https://github.com/Azure/Azurite?tab=readme-ov-file#getting-started): There you can store your pain. Also Durable Functions need a storage.
- **.NET 8**: Fetch it from the [official site](https://dotnet.microsoft.com/en-us/download#macos). No surprises there.


If you want suprises tho, you can also brew those if you want. But you will curse the day you did that because of environment paths variable shenanigans.

- **Azure Functions Core Tools Setup**: Just when you thought you were done downloading stuff:

```bash
brew tap azure/functions
brew install azure-functions-core-tools@4
# Stuck with an older version? Overwrite it like it's hot:
brew link --overwrite azure-functions-core-tools@4
```

Ignore the existential crisis that comes from brew claiming you're running an outdated version seconds after installation.

Even the official [func tools build pipeline](https://azfunc.visualstudio.com/Azure%20Functions/_build?definitionId=11&_a=summary) gave up.


## The Apple Silicon GRPC.Core Bug

If you get something like this luck was not on your side.

`````
[2024-03-17T02:47:43.675Z] A host error has occurred during startup operation '45985b1e-5ad9-4304-877e-8b3b11419509'.
[2024-03-17T02:47:43.675Z] Grpc.Core: Error loading native library. Not found in any of the possible locations: /bin/output/.azurefunctions/../../runtimes/osx-arm64/native/libgrpc_csharp_ext.arm64.dylib
Value cannot be null. (Parameter 'provider')
`````


### Fixing the Mess

Clone this repo and save yourself from my suffering. It's a no-brainer.


**Easy Fix (do this):**

This is the fix you get with this repo.

1. Slap this line into your `*.csproj`, and don't you dare deviate from the version:

   ```xml
   <PackageReference Include="Contrib.Grpc.Core.M1" Version="2.41.0" />
   ```

2. Jam this into the bottom of your `*.csproj` file:

   ```xml
     <Target Name="CopyGrpcNativeAssetsToOutDir" AfterTargets="Build">
       <ItemGroup>
         <NativeAssetToCopy Condition="$([MSBuild]::IsOSPlatform('OSX'))"
           Include="$(OutDir)runtimes/osx-arm64/native/*" />
       </ItemGroup>
       <Copy SourceFiles="@(NativeAssetToCopy)"
         DestinationFolder="$(OutDir).azurefunctions/runtimes/osx-arm64/native" />
     </Target>
   </Project>
   ```

   Peek at this [GitHub gem](https://github.com/Azure/azure-functions-durable-extension/issues/2446#issuecomment-1517203490) for the gritty details.

**Hardcore Method (if you're feeling brave or just bored):**

Embark on a quest to manually build this binary and tango with CMAKE. It's not just a step; it's a journey into madness. Check out [Grpc.Core.M1's GitHub](https://github.com/einari/Grpc.Core.M1) for your descent into DIY hell, and copy the binary into one of the folder it's searching it in.


Happy coding, and may the odds be ever in your favor!

Cheers!

