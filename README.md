# DeepseekSdkVb
VB-friendly .NET bindings for Deepseek API. It's part of the Nukepayload2 VB AI SDK as a model provider.

- [x] Enumerate result asynchronously with the syntax introduced in VB 11.
- [x] AOT compatible on .NET 8 or later
- [x] .NET Framework compatible
- [ ] WIP: Microsoft.Extension.AI integration

Code examples: [CodeExamples.vb](https://github.com/Nukepayload2/DeepseekSdkVb/blob/master/DeepseekAPIExamples/CodeExamples.vb)

Get on NuGet: [Nukepayload2.AI.Providers.Deepseek](https://www.nuget.org/packages/Nukepayload2.AI.Providers.Deepseek)

## Target Frameworks
- .NET Standard 2.0
- .NET 8 or later

## Progress
It's currently in beta stage. 
We reserve the rights of **making breaking changes**.

Do not use it in production environment unless you've tested it carefully.

### Implementation
- [x] Text completion
- [x] Text streaming
- [x] Tool call in completion
- [ ] Tool call in streaming (not supported. See the streaming response schema: https://api-docs.deepseek.com/zh-cn/api/create-chat-completion)

### Tested Manually
- [x] Text completion
- [x] Text streaming
- [x] Tool call in completion
- [ ] Tool call in streaming (ignored)

### Microsoft.Extension.AI 9.0.0 Preview
- [ ] Chat completion
- [ ] Chat streaming
- [ ] Tool call in completion
- [ ] Tool call in streaming

### Tested Manually with Microsoft.Extension.AI
- [ ] Text completion
- [ ] Text streaming
- [ ] Tool call in completion
- [ ] Tool call in streaming
