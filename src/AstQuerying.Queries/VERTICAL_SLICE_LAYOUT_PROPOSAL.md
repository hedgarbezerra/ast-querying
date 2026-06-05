# Proposta de layout vertical slice — AstQuerying.Queries

Este documento descreve a árvore atual de `ShiftsManager.Query`, o estado do repositório de destino e um mapeamento concreto para uma organização em fatias verticais. **Nenhum ficheiro foi migrado**; serve apenas para revisão.

---

## 1. Estado do destino (`ast-querying`)

O caminho `c:\Users\hedga\RiderProjects\ast-querying\AstQuerying\src\AstQuerying.Queries\` **existe**.

O projeto `AstQuerying.Queries.csproj` está presente e, neste momento, é um SDK mínimo (`net10.0`, `Nullable` ativo, sem `PackageId` nem referências declaradas). O pacote `ShiftsManager.Query` usa `Microsoft.AspNetCore.App` e `Microsoft.EntityFrameworkCore` — ao migrar, estas dependências devem ser replicadas ou afinadas no novo `.csproj`.

---

## 2. Árvore atual (apenas `.cs` de código-fonte)

Pastas sob `utilities/ShiftsManager.API/ShiftsManager.Query/` (excluindo `bin/`, `obj/`):

| Pasta | Ficheiros `.cs` |
|-------|-------------------|
| `Ast/` | `QueryNode.cs`, `ValueNode.cs` |
| `Configuration/` | `FieldRuleChain.cs`, `QueryConfiguration.cs`, `RegistrationDraft.cs` |
| `DependencyInjection/` | `QueryEngineOptions.cs`, `QueryServiceCollectionExtensions.cs` |
| `ExceptionHandling/` | `QueryExceptionHandler.cs` |
| `Exceptions/` | `QueryConfigurationException.cs`, `QueryConversionException.cs`, `QueryException.cs`, `QueryValidationException.cs` |
| `Filters/` | `FilterClauseDto.cs`, `FilterExpressionBuilder.cs`, `IFilterExpressionBuilder.cs`, `IFilterOperator.cs`, `IValueConverter.cs`, `StandardFilterOperators.cs`, `ValueConverter.cs` |
| `Functions/` | `IFunctionProvider.cs`, `SystemFunctions.cs` |
| `Internal/` | `ExpressionNormalizer.cs`, `MemberPathAnalyzer.cs`, `QueryAssemblyScanner.cs`, `QueryRegistryBuilder.cs` |
| `Metadata/` | `EntityMetadata.cs`, `IQueryMetadataProvider.cs`, `PathSegment.cs`, `PropertyMetadata.cs`, `QueryMetadataProvider.cs`, `RegistryQueryMetadataProvider.cs` |
| `Parsing/` | `QueryFilterParser.cs` |
| `Registry/` | `IQueryRegistry.cs` |
| `Resolvers/` | `IPropertyResolver.cs`, `PropertyResolver.cs` |
| `Sorting/` | `ISortBuilder.cs`, `PaginationRequestDto.cs`, `PaginationResolver.cs`, `SortBuilder.cs`, `SortFieldDto.cs` |
| `Validation/` | `IQueryValidator.cs`, `QueryValidator.cs` |

**Total:** 41 ficheiros `.cs` de origem (sem contar artefactos de build).

---

## 3. Padrão escolhido: `Abstractions/` + `Features/`

**Não** se usa um segundo top-level `Infrastructure/` para estes módulos.

- **`Abstractions/<Context>/`** concentra apenas contratos por contexto (`Validation`, `Operators` via `Filters`, `Functions`, `Filters`, `Sorting`, `Registry`, `Resolving`, `Metadata`), alinhado com o pedido e fácil de referenciar a partir de testes ou extensões.
- **`Features/<Context>/`** concentra implementações que pertencem à mesma capacidade de produto (filtros, ordenação, metadados, etc.), mantendo a fatia vertical **navegável por pasta** sem misturar DTOs com serviços.
- **`Contracts/`** e **`Configuration/`** isolam tipos partilhados (DTOs, modelos de metadados, AST, exceções) e arranque/DI/opções, evitando que `Features` vire um depósito genérico de “tudo o que não é interface”.

Alternativa **só com `Features/`** (interfaces dentro de `Features/Filters/Abstractions/` ou `Features/Filters/Contracts/`) reduz pastas no root, mas **duplica** o conceito de “contrato” em vários sítios; o trio `Configuration` / `Contracts` / `Abstractions` / `Features` fica mais claro para pacotes NuGet e para leitores novos.

---

## 4. Árvore proposta (pastas)

```text
AstQuerying.Queries/
  Configuration/
    DependencyInjection/
    Discovery/
  Contracts/
    Ast/
    Exceptions/
    Filters/
    Metadata/
    Sorting/
  Abstractions/
    Filters/
    Functions/
    Metadata/
    Parsing/
    Registry/
    Resolving/
    Sorting/
    Validation/
  Features/
    ExceptionHandling/
    Filters/
    Functions/
    Internal/
    Metadata/
    Parsing/
    Registry/
    Resolving/
    Sorting/
    Validation/
```

**Nota sobre `Abstractions/Parsing/`:** hoje não existe interface dedicada ao parser; a pasta fica reservada para um futuro `IQueryFilterParser` (ou equivalente) quando quiserem simetria total. Até lá, o código continua apenas em `Features/Parsing/`.

**Operadores:** `IFilterOperator` e operadores concretos permanecem no contexto **Filters** (não há pasta `Operators` separada no código atual). Se mais tarde existirem operadores partilhados entre filtros e outro domínio, pode extrair-se `Abstractions/Operators/`.

---

## 5. Tabela de mapeamento (cada ficheiro atual → novo caminho relativo ao projeto)

| Ficheiro atual | Novo caminho relativo |
|----------------|----------------------|
| `Ast/QueryNode.cs` | `Contracts/Ast/QueryNode.cs` |
| `Ast/ValueNode.cs` | `Contracts/Ast/ValueNode.cs` |
| `Configuration/FieldRuleChain.cs` | `Configuration/FieldRuleChain.cs` |
| `Configuration/QueryConfiguration.cs` | `Configuration/QueryConfiguration.cs` |
| `Configuration/RegistrationDraft.cs` | `Configuration/RegistrationDraft.cs` |
| `DependencyInjection/QueryEngineOptions.cs` | `Configuration/DependencyInjection/QueryEngineOptions.cs` |
| `DependencyInjection/QueryServiceCollectionExtensions.cs` | `Configuration/DependencyInjection/QueryServiceCollectionExtensions.cs` |
| `ExceptionHandling/QueryExceptionHandler.cs` | `Features/ExceptionHandling/QueryExceptionHandler.cs` |
| `Exceptions/QueryConfigurationException.cs` | `Contracts/Exceptions/QueryConfigurationException.cs` |
| `Exceptions/QueryConversionException.cs` | `Contracts/Exceptions/QueryConversionException.cs` |
| `Exceptions/QueryException.cs` | `Contracts/Exceptions/QueryException.cs` |
| `Exceptions/QueryValidationException.cs` | `Contracts/Exceptions/QueryValidationException.cs` |
| `Filters/FilterClauseDto.cs` | `Contracts/Filters/FilterClauseDto.cs` |
| `Filters/FilterExpressionBuilder.cs` | `Features/Filters/FilterExpressionBuilder.cs` |
| `Filters/IFilterExpressionBuilder.cs` | `Abstractions/Filters/IFilterExpressionBuilder.cs` |
| `Filters/IFilterOperator.cs` | `Abstractions/Filters/IFilterOperator.cs` |
| `Filters/IValueConverter.cs` | `Abstractions/Filters/IValueConverter.cs` |
| `Filters/StandardFilterOperators.cs` | `Features/Filters/StandardFilterOperators.cs` |
| `Filters/ValueConverter.cs` | `Features/Filters/ValueConverter.cs` |
| `Functions/IFunctionProvider.cs` | `Abstractions/Functions/IFunctionProvider.cs` |
| `Functions/SystemFunctions.cs` | `Features/Functions/SystemFunctions.cs` |
| `Internal/ExpressionNormalizer.cs` | `Features/Internal/ExpressionNormalizer.cs` |
| `Internal/MemberPathAnalyzer.cs` | `Features/Internal/MemberPathAnalyzer.cs` |
| `Internal/QueryAssemblyScanner.cs` | `Configuration/Discovery/QueryAssemblyScanner.cs` |
| `Internal/QueryRegistryBuilder.cs` | `Features/Registry/QueryRegistryBuilder.cs` |
| `Metadata/EntityMetadata.cs` | `Contracts/Metadata/EntityMetadata.cs` |
| `Metadata/IQueryMetadataProvider.cs` | `Abstractions/Metadata/IQueryMetadataProvider.cs` |
| `Metadata/PathSegment.cs` | `Contracts/Metadata/PathSegment.cs` |
| `Metadata/PropertyMetadata.cs` | `Contracts/Metadata/PropertyMetadata.cs` |
| `Metadata/QueryMetadataProvider.cs` | `Features/Metadata/QueryMetadataProvider.cs` |
| `Metadata/RegistryQueryMetadataProvider.cs` | `Features/Metadata/RegistryQueryMetadataProvider.cs` |
| `Parsing/QueryFilterParser.cs` | `Features/Parsing/QueryFilterParser.cs` |
| `Registry/IQueryRegistry.cs` | `Abstractions/Registry/IQueryRegistry.cs` |
| `Resolvers/IPropertyResolver.cs` | `Abstractions/Resolving/IPropertyResolver.cs` |
| `Resolvers/PropertyResolver.cs` | `Features/Resolving/PropertyResolver.cs` |
| `Sorting/ISortBuilder.cs` | `Abstractions/Sorting/ISortBuilder.cs` |
| `Sorting/PaginationRequestDto.cs` | `Contracts/Sorting/PaginationRequestDto.cs` |
| `Sorting/PaginationResolver.cs` | `Features/Sorting/PaginationResolver.cs` |
| `Sorting/SortBuilder.cs` | `Features/Sorting/SortBuilder.cs` |
| `Sorting/SortFieldDto.cs` | `Contracts/Sorting/SortFieldDto.cs` |
| `Validation/IQueryValidator.cs` | `Abstractions/Validation/IQueryValidator.cs` |
| `Validation/QueryValidator.cs` | `Features/Validation/QueryValidator.cs` |

---

## 6. Convenção de namespaces

Base do assembly: **`AstQuerying.Queries`**.

Sugestão (espelha a pasta, facilita `global using` e descoberta):

| Área no disco | Namespace sugerido |
|---------------|-------------------|
| `Configuration/` (raiz) | `AstQuerying.Queries.Configuration` |
| `Configuration/DependencyInjection/` | `AstQuerying.Queries.Configuration.DependencyInjection` |
| `Configuration/Discovery/` | `AstQuerying.Queries.Configuration.Discovery` |
| `Contracts/...` | `AstQuerying.Queries.Contracts.<Sub>` — por exemplo `AstQuerying.Queries.Contracts.Filters`, `AstQuerying.Queries.Contracts.Exceptions`, `AstQuerying.Queries.Contracts.Metadata`, `AstQuerying.Queries.Contracts.Sorting`, `AstQuerying.Queries.Contracts.Ast` |
| `Abstractions/<Context>/` | `AstQuerying.Queries.<Context>.Abstractions` — por exemplo `AstQuerying.Queries.Filters.Abstractions`, `AstQuerying.Queries.Sorting.Abstractions`, `AstQuerying.Queries.Validation.Abstractions`, `AstQuerying.Queries.Resolving.Abstractions`, `AstQuerying.Queries.Metadata.Abstractions`, `AstQuerying.Queries.Registry.Abstractions`, `AstQuerying.Queries.Functions.Abstractions`, `AstQuerying.Queries.Parsing.Abstractions` (quando existir interface) |
| `Features/<Context>/` | `AstQuerying.Queries.<Context>` — por exemplo `AstQuerying.Queries.Filters`, `AstQuerying.Queries.Sorting`, `AstQuerying.Queries.Metadata`, mantendo consistência com o domínio |

**Variante mais curta:** `AstQuerying.Queries.Abstractions.Filters` em vez de `...Filters.Abstractions` — escolher uma regra e aplicá-la em todo o pacote.

---

## 7. Sugestão de identidade NuGet (`csproj`)

| Propriedade | Valor sugerido |
|-------------|----------------|
| `PackageId` | `AstQuerying.Queries` |
| `RootNamespace` | `AstQuerying.Queries` |
| `AssemblyName` | `AstQuerying.Queries` |
| `Description` | Biblioteca de motor de consulta (filtros, ordenação, metadados, registo) para AST / API. |

Opcional: prefixo de proprietário no `PackageId` se a organização publicar vários pacotes (`Contoso.AstQuerying.Queries`).

---

## 8. Próximos passos (após aprovação do layout)

1. Atualizar namespaces e `usings` em massa (IDE ou script).
2. Ajustar `AstQuerying.Queries.csproj` com as mesmas referências necessárias que `ShiftsManager.Query.csproj`.
3. Atualizar referências de projetos consumidores no repo `docker-experiments` e em `ast-querying`.
4. Se for introduzida `IQueryFilterParser`, mover assinatura para `Abstractions/Parsing/` e manter implementação em `Features/Parsing/`.

---

## 9. Resumo

- **41** ficheiros `.cs` mapeados da árvore atual para `Configuration/`, `Contracts/`, `Abstractions/` e `Features/`.
- Destino **`ast-querying`** confirmado; `.csproj` mínimo presente.
- Padrão único: **interfaces em `Abstractions/<Context>/`**, **implementações em `Features/<Context>/`**, **tipos partilhados em `Contracts/`**, **arranque e DI em `Configuration/`** (incluindo `Discovery` para o scanner).

---

## 10. Variante mesclada (fatias na raiz) e visibilidade

Decisão atual em relação ao plano híbrido discutido:

- **Raiz por fatia** (ex.: `Ast/`, `Filters/`, `Sorting/`, …) em vez do layout antigo com `Contracts/` + `Abstractions/` + `Features/` só ao primeiro nível.
- Dentro de cada fatia, subpastas **`Contracts/`** (interfaces e outros contratos do slice), **`ValueObjects/`** (DTOs e modelos do slice), **`Implementations/`** (classes concretas).
- **`Common/`** na raiz para transversais (ex.: `Exceptions/`), mais **`Configuration/`** para DI, opções e descoberta de assemblies.
- **Visibilidade**: as classes em **`Implementations/`** ficam **`public`** (não se exige `internal`). A pasta continua a separar papel; consumidores externos devem preferir **interfaces + DI** conforme documentação do pacote.

Namespaces sugeridos: `AstQuerying.Queries.{Slice}.{Contracts|ValueObjects|Implementations}` e `AstQuerying.Queries.Common.Exceptions`, `AstQuerying.Queries.Configuration.*`.

**Nota:** o nome `Contracts/` ao nível da fatia substitui `Abstractions/`; não confundir com o `Contracts/` global da secção 4 do mesmo documento (proposta legada).

