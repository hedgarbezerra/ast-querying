# ast-querying
A strongly typed query engine for .NET and Entity Framework Core that converts structured API queries into LINQ Expression Trees. Supports configurable filtering, sorting, computed values, metadata generation, startup validation, and high-performance execution without dynamic LINQ or runtime reflection.

## Documentação

- [GitHub Packages (NuGet)](docs/github-packages-nuget.md) — consumir `AstQuerying.Queries` a partir do GitHub Packages (`nuget.config`, autenticação).
- [nuget.org — publicação confiável](docs/nuget-org-trusted-publishing.md) — OIDC/GitHub Actions e checklist para o workflow de CI/CD no nuget.org.

## Versão do pacote e publicação local

- **Incrementar a versão:** altere `<Version>` em `src/AstQuerying.Queries/AstQuerying.Queries.csproj`, ou use `dotnet pack -p:Version=1.2.3 -p:PackageVersion=1.2.3` para fixar a versão na linha de comandos. O projeto define `Version`; `PackageVersion` não está definido no ficheiro e, por omissão do SDK, alinha-se a `Version` quando não é sobrescrita.

- **Gerar o pacote localmente (exemplo):** na raiz do repositório: `dotnet pack src/AstQuerying.Queries/AstQuerying.Queries.csproj -c Release -o ./artifacts`

- **CI:** o job `pack` em `.github/workflows/publish-package.yml` executa `dotnet pack` sem `-p:Version=` nem `-p:PackageVersion=`, apenas `-p:RepositoryUrl=` e `-p:PackageProjectUrl=`, portanto a versão do pacote publicado corresponde à do `.csproj` salvo no ramo.
