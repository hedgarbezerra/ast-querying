# Publicação no nuget.org (Publicação confiável / OIDC)

O workflow `.github/workflows/publish-package.yml` publica `AstQuerying.Queries` no GitHub Packages e, nas mesmas condições (push para o ramo predefinido do repositório, não em PR), também no **nuget.org** via **Publicação confiável** (token OIDC do GitHub Actions → chave de API temporária no nuget.org).

Documentação oficial (implementação alinhada ao exemplo de GitHub Actions): [Publicação confiável no nuget.org — GitHub Actions](https://learn.microsoft.com/pt-br/nuget/nuget-org/trusted-publishing#github-actions).

Aviso: a funcionalidade pode estar a ser disponibilizada gradualmente; se não vir **Publicação confiável** na conta nuget.org, consulte a mesma página Learn.

## Nome exato do ficheiro do workflow (política no nuget.org)

Na política de publicação confiável, o campo **ficheiro de workflow** deve ser **apenas o nome do ficheiro** (sem caminho):

`publish-package.yml`

Isto corresponde ao ficheiro em `.github/workflows/publish-package.yml`.

## Checklist de configuração

1. **Política de Publicação confiável no nuget.org**
   - Proprietário do repositório GitHub: utilizador ou organização (igual ao `github.repository_owner`).
   - Nome do repositório: igual ao nome do repositório no GitHub (sem `owner/`).
   - **Ficheiro de workflow:** `publish-package.yml` (só o nome do ficheiro).
   - **Ambiente (opcional):** deixe vazio se o job de publicação **não** usar `environment:`. Se no workflow definir `environment: release` (ou outro nome), registe **o mesmo** nome de ambiente na política para restringir publicações a esse ambiente.

2. **Segredo no GitHub (nome de utilizador nuget.org)**
   - O passo `NuGet/login@v1` requer `with: user:` com o **nome de utilizador do perfil nuget.org** (não o e-mail).
   - Crie um segredo no repositório (ex.: `NUGET_ORG_USER`) e use-o no workflow como `${{ secrets.NUGET_ORG_USER }}`. **Não** coloque o nome real no repositório.

3. **Permissões dos jobs**
   - O job que corre `NuGet/login@v1` e o `dotnet nuget push` para nuget.org (`publish-nuget`) deve incluir `id-token: write`.
   - O job que faz o push para GitHub Packages (`publish-github`) deve incluir `packages: write` com `GITHUB_TOKEN`.

## Diferença em relação ao GitHub Packages

| Destino | Autenticação |
|--------|----------------|
| GitHub Packages | `GITHUB_TOKEN` com `packages: write`; `dotnet nuget push` para `https://nuget.pkg.github.com/...` |
| nuget.org | OIDC: `NuGet/login@v1` obtém chave temporária em `${{ steps.login.outputs.NUGET_API_KEY }}`; `dotnet nuget push` para `https://api.nuget.org/v3/index.json` |

## Ambiente GitHub (`environment`) — opcional

- **Sem `environment` no job:** na política nuget.org, deixe o campo de ambiente vazio.
- **Com `environment: release` (ou outro):** na política nuget.org, indique o mesmo ambiente; pode usar regras de proteção e aprovações no GitHub para controlar quem dispara a publicação.

## Ligações úteis

- [Publicação confiável — GitHub Actions (Learn)](https://learn.microsoft.com/pt-br/nuget/nuget-org/trusted-publishing#github-actions)
- [Trabalhar com o registo NuGet do GitHub Packages](https://docs.github.com/pt/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry)
