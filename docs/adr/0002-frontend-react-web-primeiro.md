# ADR 0002 — Frontend: React Web (web primeiro, mobile depois)

**Data:** 2026-06-15
**Status:** Aceito

## Decisão

Web com React + Vite (JavaScript puro, sem TypeScript) como primeira interface.
Mobile (React Native + Expo) planejado para Fase 6b após o web estar estável.

## Contexto

Precisamos de interface visual que o usuário e o contador consigam usar sem abrir o Swagger.
O Contador acessa principalmente por PC. O usuário quer mobilidade no futuro.

## Alternativas descartadas

- **Blazor WASM** — C# no front seria consistente com o .NET do backend, mas o ecossistema React é maior e o usuário prefere JS/CSS/HTML.
- **Mobile primeiro** — Mais complexo para distribuir; o web serve os dois perfis igualmente e não exige instalação.
- **TypeScript** — Adicionaria fricção desnecessária para um projeto solo sem equipe grande.

## Consequências

- Frontend em `C:\git-trabalhos\FinanceiroAPI\frontend\` (subdiretório do repo)
- Dev server em `http://localhost:5173` (já na allowlist do CORS do backend)
- Para produção futura: build estático servido pelo próprio ASP.NET Core ou CDN
- Fase 6b (mobile React Native) reutilizará a mesma API sem mudanças
