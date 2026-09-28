# RankingPreProva

Plataforma gamificada de preparação para concursos públicos: banco global de questões, provas criadas pela comunidade, rankings, XP, níveis, sequência de estudos e conquistas.

## Stack

- **.NET 10** · Blazor Web App com renderização **Interactive WebAssembly** global (sem prerender)
- **RankingPreProvaServer**: host, Minimal APIs (`/api/*`), autenticação Google OAuth + cookie, EF Core + PostgreSQL
- **RankingPreProvaClient**: páginas Blazor WASM com **FluentUI Blazor 4**
- **RankingPreProva.Shared**: DTOs, enums e utilitários compartilhados

## Primeiros passos

1. Configure o PostgreSQL em `appsettings.json` (`ConnectionStrings:DefaultConnection`).
2. Aplique as migrations:

   ```powershell
   dotnet ef database update --project RankingPreProvaServer
   ```

3. Configure o login com Google (veja abaixo) e execute o projeto `RankingPreProvaServer`.

> **Sem credenciais do Google em Development**, o app usa um login simulado:
> `/account/login-dev?n=1` entra como "Usuário Dev 1" (administrador). Use `n=2`, `n=3`… para simular outros usuários e testar rankings.

## Login com Google (OAuth)

1. No [Google Cloud Console](https://console.cloud.google.com/apis/credentials), crie um **ID do cliente OAuth** do tipo *Aplicativo da Web*.
2. Adicione o URI de redirecionamento autorizado: `https://localhost:7101/signin-google` (e o domínio de produção).
3. Registre os segredos com user-secrets:

   ```powershell
   cd RankingPreProvaServer
   dotnet user-secrets init
   dotnet user-secrets set "Authentication:Google:ClientId" "SEU_CLIENT_ID"
   dotnet user-secrets set "Authentication:Google:ClientSecret" "SEU_CLIENT_SECRET"
   ```

## Moderadores

Os e-mails em `Admin:Emails` recebem o papel de moderador no próximo login e passam a ter acesso a `/admin/denuncias`:

```powershell
dotnet user-secrets set "Admin:Emails:0" "voce@gmail.com"
```

## Google AdSense

Os espaços de anúncio (coluna lateral, rodapé e blocos no conteúdo) mostram um placeholder até serem configurados:

```powershell
dotnet user-secrets set "AdSense:ClientId" "ca-pub-XXXXXXXXXXXXXXXX"
dotnet user-secrets set "AdSense:Slots:Lateral" "1234567890"
dotnet user-secrets set "AdSense:Slots:Rodape" "1234567890"
dotnet user-secrets set "AdSense:Slots:Conteudo" "1234567890"
```

Atualize também `RankingPreProvaServer/wwwroot/ads.txt` com o seu `pub-id`. Durante a resolução de provas, o anúncio do rodapé é ocultado para não distrair.

## Regras principais

- Questões de **múltipla escolha (A–E)** ou **certo/errado (C/E)**, com uma única resposta correta.
- Só o autor edita a própria questão ou prova. Itens já usados são **arquivados** em vez de excluídos.
- Provas com tentativas registradas têm questões e regras **congeladas**, para preservar o ranking.
- Ranking da prova: **só a primeira tentativa** de cada usuário vale. Ordena por maior nota e, no empate, menor tempo.
- Pontuação configurável: simples, estilo Cebraspe (erro desconta) ou com peso por questão.
- Votos: itens com saldo negativo vão para o fim das listas e aparecem com baixa opacidade.
- Denúncias: com 5 ou mais denúncias abertas, o item entra em revisão automaticamente.