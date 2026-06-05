# Consumir `AstQuerying.Queries` no GitHub Packages (NuGet)

O workflow publica o pacote NuGet no registro GitHub Packages. A primeira publicação fica **privada por predefinição**. Definir `RepositoryUrl` / `PackageProjectUrl` no pacote (o CI passa estes valores ao `pack`) ajuda a associar o pacote ao repositório no GitHub.

Documentação oficial: [Trabalhar com o registro NuGet do GitHub Packages](https://docs.github.com/pt/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry).

## Autenticação (máquina local ou CI)

Crie ou edite um `nuget.config` na solução ou na pasta do utilizador (por exemplo `%AppData%\NuGet\NuGet.Config` no Windows, `~/.nuget/NuGet/NuGet.Config` no Linux/macOS). **Não comite** tokens nem ficheiros com credenciais.

Substitua:

- `USERNAME` — nome de utilizador GitHub (ou qualquer valor não vazio, consoante a política da organização; com PAT costuma ser o nome de utilizador).
- `TOKEN` — PAT clássico com âmbito `read:packages` (e `write:packages` se for publicar fora do GitHub Actions).
- `NAMESPACE` — proprietário do repositório no GitHub (utilizador ou organização), o mesmo segmento usado em `https://nuget.pkg.github.com/NAMESPACE/index.json`.

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="github" value="https://nuget.pkg.github.com/NAMESPACE/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="USERNAME" />
      <add key="ClearTextPassword" value="TOKEN" />
    </github>
  </packageSourceCredentials>
</configuration>
```

No GitHub Actions, o push usa `GITHUB_TOKEN` com permissão `packages: write`; não é necessário PAT no repositório para esse fluxo.

## Publicação também no nuget.org (OIDC)

O mesmo workflow pode enviar o `.nupkg` para **nuget.org** com **Publicação confiável** (sem chave de API de longa duração no repositório). O fluxo usa `NuGet/login@v1` e `id-token: write`; o nome de utilizador do perfil nuget.org deve estar num segredo do GitHub.

Checklist, nome exato do ficheiro do workflow para a política no nuget.org e opção `environment`: ver [nuget-org-trusted-publishing.md](./nuget-org-trusted-publishing.md).

## Referência ao pacote no projeto

No `.csproj` (ou `Directory.Packages.props`), use o `PackageId` publicado (por exemplo `AstQuerying.Queries`) e a versão exata ou intervalo desejado:

```xml
<ItemGroup>
  <PackageReference Include="AstQuerying.Queries" Version="1.0.0" />
</ItemGroup>
```

Ajuste `Version` à versão publicada no GitHub Packages.

## NuGet.org e GitHub Packages ao mesmo tempo

Se misturar `nuget.org` com a origem GitHub e aparecerem erros **403** ou pacotes resolvidos na origem errada, configure **mapeamento de origens de pacotes** (`packageSourceMapping`) conforme a documentação do GitHub e do NuGet, para que `AstQuerying.Queries` (e o prefixo da organização) mapeiem apenas para a origem `github`.
