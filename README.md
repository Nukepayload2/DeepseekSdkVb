# DeepseekSdkVb
VB-friendly .NET bindings for Deepseek API. It's part of the Nukepayload2 VB AI SDK as a model provider.

- [x] Enumerate result asynchronously with the syntax introduced in VB 11.
- [x] AOT compatible on .NET 8 or later
- [x] .NET Framework compatible
- [x] Microsoft.Extension.AI integration

Code examples: 
- [CodeExamples.vb](https://github.com/Nukepayload2/DeepseekSdkVb/blob/master/DeepseekAPIExamples/CodeExamples.vb)
- [MsAICodeExamples.vb](https://github.com/Nukepayload2/DeepseekSdkVb/blob/master/DeepseekAPIExamples/MsAICodeExamples.vb)

Get on NuGet: [Nukepayload2.AI.Providers.Deepseek](https://www.nuget.org/packages/Nukepayload2.AI.Providers.Deepseek)

## Target Frameworks
- .NET Standard 2.0
- .NET 8 or later

## Progress
It's currently in release candidate stage. 
We reserve the rights of **making breaking changes** before RTM.

### Implementation
- [x] Text completion
- [x] Text streaming
- [x] Tool call in completion
- [x] Tool call in streaming (requires DeepSeek-V3-0324 or later)
- [x] Fill-In-the-Middle completion (beta)
- [x] List models
- [x] Get balance

### Tested Manually
- [x] Text completion
- [x] Text streaming
- [x] Tool call in completion
- [x] Tool call in streaming
- [x] Fill-In-the-Middle completion (beta)
- [x] List models
- [x] Get balance

### Microsoft.Extension.AI 9.0.3 Preview
- [x] Chat completion
- [x] Chat streaming
- [x] Tool call in completion
- [x] Tool call in streaming

### Tested Manually with Microsoft.Extension.AI
- [x] Text completion
- [x] Text streaming
- [x] Tool call in completion
- [x] Tool call in streaming
