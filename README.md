# ast-querying

**[PT-BR]** Pacote .NET (`AstQuerying.Queries`) que expõe um motor de consultas tipado: pedidos em JSON (filtro em
árvore, ordenação, paginação) validados contra regras declarativas e compilados para expressões LINQ sobre `IQueryable`,
sem reflexão no caminho quente.

## Coautoria com IA

Este repositório foi coautorado com IA.

## Aviso importante

> **Nota:** Este repositório **não será mantido**. Foi criado para **implementar e experimentar** o mecanismo de
> consulta e para **testar a publicação** de pacotes NuGet no **GitHub Packages** e no **nuget.org**. Não conte com
> evolução contínua, correções planeadas ou suporte.

## Visão geral do pacote

O `AstQuerying.Queries` destina-se a consumidores externos como biblioteca NuGet: converte pedidos estruturados em
árvores de expressão LINQ, com filtragem configurável, ordenação, funções dinâmicas em operandos, metadados derivados do
registo e validação na construção do `IQueryRegistry` (arranque da aplicação). O fluxo em tempo de pedido combina
análise do JSON, validação semântica contra metadados e construção de expressões; não utiliza LINQ dinâmico nem reflexão
no caminho crítico de construção do predicado.

## Registo de serviços e pipeline HTTP

Extensões em `AstQuerying.Queries.Configuration.DependencyInjection`:

- `AddQueryEngine` — regista o motor, incluindo `IQueryRegistry` singleton construído a partir do varrimento de
  assemblies à procura de subclasses de `QueryConfiguration<TEntity>`, mais operadores e funções integrados definidos em
  `QueryOperatorsRegistry` e `QueryFunctionsRegistry`.
- `AddQueryExceptionHandler` — regista `AddProblemDetails`, `QueryExceptionHandler` (`AstQuerying.Queries.Handlers`)
  como `IExceptionHandler` e escreve `QueryException` (e derivados) como RFC 7807 Problem Details (
  `application/problem+json`) com o código HTTP e detalhes da exceção.

Na aplicação ASP.NET Core, após `AddQueryExceptionHandler`, inclua `app.UseExceptionHandler();` no pipeline para que o
handler seja invocado.

Exemplo mínimo:

```csharp
using AstQuerying.Queries.Configuration.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddQueryEngine(typeof(Program).Assembly);
builder.Services.AddQueryExceptionHandler();

var app = builder.Build();
app.UseExceptionHandler();
```

Passe os assemblies onde vivem as classes `QueryConfiguration<TEntity>` (por exemplo `typeof(Program).Assembly`). Sem
assemblies adicionais, o assembly de chamada de `AddQueryEngine` é incluído por omissão.

## Configurar e utilizar o motor

Não existe uma única API “executar query”: no arranque regista-se o motor no DI; em cada pedido o anfitrião compõe *
*parser → validação → builders** e aplica o resultado a um `IQueryable<T>`.

### Passos de configuração (arranque)

1. `AddQueryEngine(...)` com os assemblies que contêm `QueryConfiguration<TEntity>`.
2. Uma `QueryConfiguration<TEntity>` por entidade, com `RuleFor` / `RuleForCollection` a cobrir todas as propriedades
   públicas de instância.
3. Opcional em ASP.NET Core: `AddQueryExceptionHandler` e `app.UseExceptionHandler()` para mapear `QueryException` a
   Problem Details.

### Passos por pedido (filtro e predicado)

1. Deserializar JSON para `FilterClauseDto` (`AstQuerying.Queries.Filters.Dtos`).
2. `var node = QueryFilterParser.Parse(filterDto);`
3. Resolver `IQueryValidator` e chamar `Validate(typeof(TEntity), node)`.
4. Resolver `IFilterExpressionBuilder` e chamar `Build<TEntity>(node)` → `Expression<Func<TEntity, bool>>` para usar em
   `Queryable.Where(queryable, predicate)`, ou resolver `IFilteringService` e chamar `Apply<TEntity>(queryable, node)`
   após validar o AST.

Ordenação: `SortFieldDto` em `AstQuerying.Queries.Sorting.Dtos`; `ISortingKeyBuilder.Build<TEntity>` gera o selector por
campo; `ISortingService.Apply<TEntity>` aplica a lista de ordenação ao `IQueryable` após validar os campos. Paginação:
`PaginationDto` com `page` (≥ 1) e `size`; `IPaginationResolver` (registado no DI com `QueryEngineOptions`) —
`Resolve(dto)` devolve `(Skip, Take)` para encadear em `Skip`/`Take` no `IQueryable`.

### Opções (`QueryEngineOptions`)

```csharp
using AstQuerying.Queries.Configuration.DependencyInjection;

builder.Services.AddQueryEngine(o =>
{
    o.MaxPageSize = 200;
    o.AssembliesToScan.Add(typeof(Program).Assembly);
});
```

`QueryEngineOptions`: `AssembliesToScan`, `MaxSkip` (predefinição 50_000), `MaxPageSize` (predefinição 500),
`DefaultPageSize` (predefinição 50). `IPaginationResolver.Resolve(PaginationDto?)` usa estes limites injectados e
devolve `(Skip, Take)` com página **um-based** e propriedade JSON `size`.

## Forma JSON alinhada aos DTOs

O pacote não impõe um único DTO de pedido HTTP; o anfitrião deserializa para os tipos em
`AstQuerying.Queries.Filters.Dtos`, `AstQuerying.Queries.Sorting.Dtos`, etc. Os nomes JSON seguem `[JsonPropertyName]`:

**Filtro** — `FilterClauseDto`: `and`, `or`, `not` (árvore); folha com `field`, `operator`, `value` (elemento JSON) ou
`values` (lista) para `in` / `notIn`. Operadores reconhecidos (nomes case-insensitive no registo) incluem: `=`, `!=`,
`>`, `>=`, `<`, `<=`, `contains`, `startsWith`, `endsWith`, `in`, `notIn`, `isNull`, `isNotNull`.

**Ordenação** — lista de `SortFieldDto`: `field`, `direction` em JSON como `"asc"` ou `"desc"`; em C# o tipo é
`SortDirection` e a omissão corresponde a descendente.

**Paginação** — `PaginationDto`: `page` (≥ 1, omissão = 1) e `size` (omissão = `DefaultPageSize`).

Valores dinâmicos em operandos: objeto JSON com `function` ou `$function` e nome da função registada (por exemplo
`Today`, `Now`, `StartOfMonth`, `EndOfMonth`, `CurrentYear`, `LegalAge`, `MinimumWage`).

Exemplo ilustrativo de corpo JSON (o anfitrião pode aninhar estes fragmentos como preferir):

```json
{
  "filter": {
    "and": [
      { "field": "Name", "operator": "contains", "value": "ali" },
      { "field": "Age", "operator": ">", "value": 18 }
    ]
  },
  "sort": [{ "field": "Name", "direction": "asc" }],
  "pagination": { "page": 1, "size": 20 }
}
```

### Referência rápida das propriedades JSON

| Conceito  | Propriedades JSON principais                   |
|-----------|------------------------------------------------|
| Nó lógico | `and`, `or`, `not` (arrays ou objeto aninhado) |
| Folha     | `field`, `operator`, `value` ou `values`       |
| Ordenação | `field`, `direction`                           |
| Paginação | `page`, `size`                                 |

Limites numéricos da paginação configuram-se em `QueryEngineOptions` (`MaxSkip`, `MaxPageSize`, `DefaultPageSize`).

## `QueryConfiguration<TEntity>`

Subclasse abstrata com `Configure()` onde se declaram regras. Cada propriedade pública de instância de `TEntity` tem de
ter regra (`RuleFor` ou `RuleForCollection`). Exemplo alinhado aos testes do repositório:

```csharp
using AstQuerying.Queries.Configuration;
using AstQuerying.Queries.Filters.Implementations;
using AstQuerying.Queries.Filters.Operators;

public sealed class PersonQueryConfiguration : QueryConfiguration<Person>
{
    protected override void Configure()
    {
        RuleFor(p => p.Id).Filtering(f => f.Operators(o => o.Allow<EqualOperator>()));
        RuleFor(p => p.Name)
            .Filtering(f => f.Operators(o =>
                o.Allow<EqualOperator>().Allow<ContainsOperator>().Allow<StartsWithOperator>()))
            .Sorting(s => s.AllowAscending().AllowDescending());
        RuleFor(p => p.Age)
            .Filtering(f => f.Operators(o => o.Allow<EqualOperator>().Allow<GreaterThanOperator>()))
            .Sorting(s => s.AllowAscending().AllowDescending());
    }
}
```

Navegações e coleções: `RuleFor` segue uma cadeia de propriedades; `RuleForCollection<TItem>` suporta **uma**
profundidade de coleção (sem coleções aninhadas). Ordenação e operadores por campo configuram-se com `Filtering` /
`Sorting`. `DefaultSorting` e `DefaultFilter` definem valores por defeito quando o cliente omite sort ou filtro.

## Como o motor funciona

1. **Arranque:** `QueryRegistryBuilder` lê todas as `QueryConfiguration<TEntity>` nos assemblies configurados, valida
   regras (incluindo a exigência de uma regra por propriedade pública de instância em `TEntity`), valida que todas as
   implementações integradas de operadores e funções estão registadas, e publica `IQueryRegistry` com `EntityMetadata`,
   operadores e funções.
2. **Pedido:** o anfitrião obtém `IFilterExpressionBuilder`, `IFilteringService`, `ISortingKeyBuilder`,
   `ISortingService`, `IPaginationResolver`, `IQueryValidator`, etc.; faz `QueryFilterParser.Parse` sobre o
   `FilterClauseDto`; valida o AST contra o registo; compila predicados e ordenações como expressões fortemente tipadas.
3. **EF Core:** o pacote referencia `Microsoft.EntityFrameworkCore` para cenários típicos; as expressões compõem-se
   sobre `IQueryable<T>` e o provedor EF traduz quando a fonte for um `DbSet`.

## Fluxo de pedido no anfitrião

1. Deserializar JSON para `FilterClauseDto`, lista de `SortFieldDto`, `PaginationDto` (ou `null` para usar só
   predefinições de página/tamanho).
2. `QueryFilterParser.Parse(filterDto)` → AST (`QueryNode`).
3. `IQueryValidator.Validate(typeof(TEntity), node)` (e validações análogas para ordenação).
4. `IFilterExpressionBuilder.Build<TEntity>(node)` → `Expression<Func<TEntity, bool>>` e `Where`, ou
   `IFilteringService.Apply<TEntity>(queryable, node)`.
5. `ISortingService.Apply<TEntity>` com a lista de `SortFieldDto` (usa internamente `ISortingKeyBuilder.Build<TEntity>`
   por campo); `IPaginationResolver.Resolve(paginationDto)` para `Skip`/`Take`.

Metadados expostos: `IQueryMetadataProvider` / `IQueryMetadataProvider<TEntity>` e `IQueryRegistry.Entities`.

### Funções em operandos

`QueryFilterParser` interpreta um valor JSON objeto com `function` ou `$function` como `FunctionValueNode`. Funções
integradas (constante `Key` em cada tipo em `QueryFunctionsRegistry`): `Today`, `Now`, `StartOfMonth`, `EndOfMonth`,
`CurrentYear`, `LegalAge`, `MinimumWage`. O validador exige compatibilidade entre a “família” da função (temporal,
inteira de política, decimal de política) e o tipo da propriedade; o conjunto permitido por propriedade restringe-se
ainda com `Filtering(f => f.Functions(...))` quando aplicável.

## Pastas públicas e papéis

- **`Configuration`** — `QueryConfiguration<TEntity>` e regras (`RuleFor`, `RuleForCollection`, filtros, ordenação,
  valores por defeito).
- **`Configuration.DependencyInjection`** — `AddQueryEngine`, `AddQueryExceptionHandler`, `QueryEngineOptions`.
- **`Filters`**, **`Sorting`**, **`Metadata`**, **`Registry`**, **`Resolving`**, **`Validation`**, **`Parsing`**, *
  *`Functions`**, **`Ast`** — por convenção, contratos em `Contracts`, implementações em `Implementations`; DTOs de API
  em `Filters/Dtos` e `Sorting/Dtos`; outros tipos imutáveis em `ValueObjects` onde aplicável.
- **`Handlers`** — `QueryExceptionHandler` para ASP.NET Core (Problem Details a partir de `QueryException`).
- **`Common`** — exceções (`QueryException` e derivados).

## Expectativas

- A configuração é **opt-in** por entidade: cada `TEntity` exposto a consultas precisa de uma
  `QueryConfiguration<TEntity>` coerente com o modelo.
- Erros de configuração surgem no arranque (`QueryConfigurationException`); erros de pedido em tempo de execução mapeiam
  para `QueryValidationException`, `QueryConversionException`, etc., com códigos HTTP definidos nas exceções. Em
  `QueryException` e derivados, **`Reason`** é um código fixo por tipo (constantes `ReasonCode` em cada classe, por
  exemplo `QueryValidation`); **`Message`** (e o campo `detail` em Problem Details) descreve o caso concreto.

## Versão do pacote e `dotnet pack`

A versão publicada segue `<Version>` em `src/AstQuerying.Queries/AstQuerying.Queries.csproj`. O workflow [
`.github/workflows/publish-package.yml`](.github/workflows/publish-package.yml) executa `dotnet pack` sem sobrescrever
`Version`/`PackageVersion` na linha de comandos. Empacotamento local de exemplo:
`dotnet pack src/AstQuerying.Queries/AstQuerying.Queries.csproj -c Release -o ./artifacts`.

## Documentação

- [Instalação do pacote NuGet e publicação (feeds, CI, OIDC)](docs/consume-package.md) — sem descrição da API; ver
  secções deste README.

## Publicação (remissão)

O mesmo workflow publica o `.nupkg` no GitHub Packages e no nuget.org em `push` para o ramo predefinido (`main`), após
`build-test` e `pack`. Passos de publicação (segredos, permissões, OIDC no nuget.org) estão em [
`docs/consume-package.md`](docs/consume-package.md); a utilização da biblioteca e o comportamento do motor estão neste
README.

---

## English

**Description:** The `AstQuerying.Queries` .NET package provides a strongly typed query engine: JSON-shaped requests (
filter tree, sort, pagination) are validated against declarative rules and compiled to LINQ expressions over
`IQueryable`, without reflection on the hot path.

## AI co-authorship

This repository was co-authored with AI.

## Important notice

> **Note:** This repository is **not maintained**. It was built to **implement and experiment** with the query mechanism
> and to **test publishing** NuGet packages to **GitHub Packages** and **nuget.org**. Do not expect ongoing development,
> planned fixes, or support.

## Package overview

`AstQuerying.Queries` is meant for third-party consumption as a NuGet library: it turns structured requests into LINQ
expression trees with configurable filtering, sorting, dynamic operand functions, metadata derived from registration,
and validation when `IQueryRegistry` is built at application startup. Per-request work parses JSON, validates against
metadata, and builds expressions without dynamic LINQ or reflection on the predicate hot path.

## Service registration and HTTP pipeline

Extensions in `AstQuerying.Queries.Configuration.DependencyInjection`:

- `AddQueryEngine` — registers the engine, including a singleton `IQueryRegistry` built by scanning assemblies for
  `QueryConfiguration<TEntity>` subclasses, plus built-in operators and functions from `QueryOperatorsRegistry` and
  `QueryFunctionsRegistry`.
- `AddQueryExceptionHandler` — registers `AddProblemDetails`, `QueryExceptionHandler` (`AstQuerying.Queries.Handlers`)
  as `IExceptionHandler`, writing `QueryException` (and derived types) as RFC 7807 Problem Details (
  `application/problem+json`) with the HTTP status code and exception details.

In ASP.NET Core, after `AddQueryExceptionHandler`, call `app.UseExceptionHandler()` so the handler runs.

Minimal example:

```csharp
using AstQuerying.Queries.Configuration.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddQueryEngine(typeof(Program).Assembly);
builder.Services.AddQueryExceptionHandler();

var app = builder.Build();
app.UseExceptionHandler();
```

Pass assemblies that contain your `QueryConfiguration<TEntity>` types (for example `typeof(Program).Assembly`). With no
extra assemblies, the calling assembly of `AddQueryEngine` is included by default.

## Configure and use the engine

There is no single “run query” API: at startup you register services in DI; on each request the host composes **parser →
validation → builders** and applies the result to an `IQueryable<T>`.

### Configuration (startup)

1. `AddQueryEngine(...)` with assemblies that contain `QueryConfiguration<TEntity>`.
2. One `QueryConfiguration<TEntity>` per queryable entity, with `RuleFor` / `RuleForCollection` covering every public
   instance property.
3. Optional in ASP.NET Core: `AddQueryExceptionHandler` and `app.UseExceptionHandler()` to map `QueryException` to
   Problem Details.

### Per-request steps (filter and predicate)

1. Deserialize JSON to `FilterClauseDto` (`AstQuerying.Queries.Filters.Dtos`).
2. `var node = QueryFilterParser.Parse(filterDto);`
3. Resolve `IQueryValidator` and call `Validate(typeof(TEntity), node)`.
4. Resolve `IFilterExpressionBuilder` and call `Build<TEntity>(node)` → `Expression<Func<TEntity, bool>>` for
   `Queryable.Where(queryable, predicate)`, or resolve `IFilteringService` and call `Apply<TEntity>(queryable, node)`
   after validating the AST.

Sort: `SortFieldDto` in `AstQuerying.Queries.Sorting.Dtos`; `ISortingKeyBuilder.Build<TEntity>` produces the key
selector per field; `ISortingService.Apply<TEntity>` applies the sort list to the `IQueryable` after validating fields.
Pagination: `PaginationDto` with `page` (≥ 1) and `size`; `IPaginationResolver` (registered with injected
`QueryEngineOptions`) — `Resolve(dto)` returns `(Skip, Take)` to chain as `Skip`/`Take` on the `IQueryable`.

### Options (`QueryEngineOptions`)

```csharp
using AstQuerying.Queries.Configuration.DependencyInjection;

builder.Services.AddQueryEngine(o =>
{
    o.MaxPageSize = 200;
    o.AssembliesToScan.Add(typeof(Program).Assembly);
});
```

`QueryEngineOptions`: `AssembliesToScan`, `MaxSkip` (default 50_000), `MaxPageSize` (default 500), `DefaultPageSize` (
default 50). `IPaginationResolver.Resolve(PaginationDto?)` uses those injected limits and returns `(Skip, Take)` with *
*one-based** `page` and JSON property `size`.

## JSON shapes aligned with DTOs

The package does not enforce one root HTTP DTO; the host deserializes into types under
`AstQuerying.Queries.Filters.Dtos`, `AstQuerying.Queries.Sorting.Dtos`, etc. JSON names follow `[JsonPropertyName]`:

**Filter** — `FilterClauseDto`: `and`, `or`, `not` (tree); leaves use `field`, `operator`, `value` (JSON element) or
`values` (array) for `in` / `notIn`. Operator names resolved case-insensitively include: `=`, `!=`, `>`, `>=`, `<`,
`<=`, `contains`, `startsWith`, `endsWith`, `in`, `notIn`, `isNull`, `isNotNull`.

**Sort** — list of `SortFieldDto`: `field`, `direction` in JSON as `"asc"` or `"desc"`; in C# the property type is
`SortDirection` with default descending.

**Pagination** — `PaginationDto`: `page` (≥ 1, default 1) and `size` (default `DefaultPageSize`).

Dynamic operand values: JSON object with `function` or `$function` and the registered function name (for example
`Today`, `Now`, `StartOfMonth`, `EndOfMonth`, `CurrentYear`, `LegalAge`, `MinimumWage`).

Illustrative JSON body (the host may nest these fragments as needed):

```json
{
  "filter": {
    "and": [
      { "field": "Name", "operator": "contains", "value": "ali" },
      { "field": "Age", "operator": ">", "value": 18 }
    ]
  },
  "sort": [{ "field": "Name", "direction": "asc" }],
  "pagination": { "page": 1, "size": 20 }
}
```

### Quick JSON property reference

| Concept      | Main JSON properties                         |
|--------------|----------------------------------------------|
| Logical node | `and`, `or`, `not` (arrays or nested object) |
| Leaf         | `field`, `operator`, `value` or `values`     |
| Sort         | `field`, `direction`                         |
| Pagination   | `page`, `size`                               |

Pagination limits are configured on `QueryEngineOptions` (`MaxSkip`, `MaxPageSize`, `DefaultPageSize`).

## `QueryConfiguration<TEntity>`

Abstract subclass with `Configure()` where rules are declared. Every public instance property on `TEntity` must have a
rule (`RuleFor` or `RuleForCollection`). Example aligned with this repository’s tests:

```csharp
using AstQuerying.Queries.Configuration;
using AstQuerying.Queries.Filters.Implementations;
using AstQuerying.Queries.Filters.Operators;

public sealed class PersonQueryConfiguration : QueryConfiguration<Person>
{
    protected override void Configure()
    {
        RuleFor(p => p.Id).Filtering(f => f.Operators(o => o.Allow<EqualOperator>()));
        RuleFor(p => p.Name)
            .Filtering(f => f.Operators(o =>
                o.Allow<EqualOperator>().Allow<ContainsOperator>().Allow<StartsWithOperator>()))
            .Sorting(s => s.AllowAscending().AllowDescending());
        RuleFor(p => p.Age)
            .Filtering(f => f.Operators(o => o.Allow<EqualOperator>().Allow<GreaterThanOperator>()))
            .Sorting(s => s.AllowAscending().AllowDescending());
    }
}
```

Navigations and collections: `RuleFor` chains members; `RuleForCollection<TItem>` supports **one** collection depth (no
nested collections). Per-field operators and sort permissions use `Filtering` / `Sorting`. `DefaultSorting` and
`DefaultFilter` apply when the client omits sort or filter.

## How the engine works

1. **Startup:** `QueryRegistryBuilder` loads every `QueryConfiguration<TEntity>` from configured assemblies, validates
   rules (including one rule per public instance property on `TEntity`), validates that all built-in operator and
   function implementations are registered, and publishes `IQueryRegistry` with `EntityMetadata`, operators, and
   functions.
2. **Request:** the host resolves `IFilterExpressionBuilder`, `IFilteringService`, `ISortingKeyBuilder`,
   `ISortingService`, `IPaginationResolver`, `IQueryValidator`, etc.; runs `QueryFilterParser.Parse` on
   `FilterClauseDto`; validates the AST against the registry; compiles predicates and sort keys as strongly typed
   expressions.
3. **EF Core:** the package references `Microsoft.EntityFrameworkCore` for typical scenarios; expressions compose on
   `IQueryable<T>` and the EF provider translates when the source is a `DbSet`.

## Request-time flow in the host

1. Deserialize JSON to `FilterClauseDto`, `List<SortFieldDto>`, `PaginationDto` (or `null` to use default page and size
   only).
2. `QueryFilterParser.Parse(filterDto)` → AST (`QueryNode`).
3. `IQueryValidator.Validate(typeof(TEntity), node)` (and analogous validation for sort).
4. `IFilterExpressionBuilder.Build<TEntity>(node)` → `Expression<Func<TEntity, bool>>` and `Where`, or
   `IFilteringService.Apply<TEntity>(queryable, node)`.
5. `ISortingService.Apply<TEntity>` with the `SortFieldDto` list (uses `ISortingKeyBuilder.Build<TEntity>` per field
   internally); `IPaginationResolver.Resolve(paginationDto)` for `Skip`/`Take`.

Metadata: `IQueryMetadataProvider` / `IQueryMetadataProvider<TEntity>` and `IQueryRegistry.Entities`.

### Functions in operands

`QueryFilterParser` treats a JSON object value with `function` or `$function` as a `FunctionValueNode`. Built-in keys (
`Key` constant on each type in `QueryFunctionsRegistry`): `Today`, `Now`, `StartOfMonth`, `EndOfMonth`, `CurrentYear`,
`LegalAge`, `MinimumWage`. Validation enforces a match between the function family (temporal, integral policy,
decimal-scale policy) and the property leaf type; allowed functions per property are still narrowed with
`Filtering(f => f.Functions(...))` when needed.

## Public folders and roles

- **`Configuration`** — `QueryConfiguration<TEntity>` and rules (`RuleFor`, `RuleForCollection`, filters, sort,
  defaults).
- **`Configuration.DependencyInjection`** — `AddQueryEngine`, `AddQueryExceptionHandler`, `QueryEngineOptions`.
- **`Filters`**, **`Sorting`**, **`Metadata`**, **`Registry`**, **`Resolving`**, **`Validation`**, **`Parsing`**, *
  *`Functions`**, **`Ast`** — by convention, contracts under `Contracts` and implementations under `Implementations`;
  API DTOs under `Filters/Dtos` and `Sorting/Dtos`; other immutable shapes under `ValueObjects` where applicable.
- **`Handlers`** — `QueryExceptionHandler` for ASP.NET Core (Problem Details from `QueryException`).
- **`Common`** — exceptions (`QueryException` and derived types).

## Expectations

- Configuration is **opt-in** per entity: each queryable `TEntity` needs a consistent `QueryConfiguration<TEntity>`.
- Configuration failures surface at startup (`QueryConfigurationException`); invalid client input maps to
  `QueryValidationException`, `QueryConversionException`, and related types with HTTP status codes on the exceptions. On
  `QueryException` and derived types, **`Reason`** is a stable per-type code (see `ReasonCode` on each class, for
  example `QueryValidation`); **`Message`** (and the Problem Details `detail` field) describes the specific failure.

## Package version and `dotnet pack`

Published versions follow `<Version>` in `src/AstQuerying.Queries/AstQuerying.Queries.csproj`. The [
`.github/workflows/publish-package.yml`](.github/workflows/publish-package.yml) workflow runs `dotnet pack` without
overriding `Version` / `PackageVersion` on the command line. Example local pack:
`dotnet pack src/AstQuerying.Queries/AstQuerying.Queries.csproj -c Release -o ./artifacts`.

## Documentation

- [NuGet package install and publishing (feeds, CI, OIDC)](docs/consume-package.md) — no API description; see sections
  in this README.

## Publishing (pointers)

The same workflow publishes the `.nupkg` to GitHub Packages and nuget.org on `push` to the default branch (`main`),
after `build-test` and `pack`. Publishing steps (secrets, permissions, OIDC on nuget.org) are in [
`docs/consume-package.md`](docs/consume-package.md); library usage and engine behavior are in this README.
