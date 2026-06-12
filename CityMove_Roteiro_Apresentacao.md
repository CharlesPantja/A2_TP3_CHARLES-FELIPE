# CityMove — Roteiro de Demonstração

**Projeto:** CityMove – Transporte Urbano Inteligente
**Dupla:** Charles Pantoja & Felipe Ferreira · Tópicos III

---

## 0. Antes de começar (preparação)

1. Abra o projeto no VS Code com **dois terminais**.
2. (Opcional, para dados limpos) Recrie o banco:
   ```
   dotnet ef database drop -f -p CityMove.Infrastructure -s CityMove.API
   ```
3. **Terminal 1 — API:** `dotnet run --project CityMove.API` → espere `Now listening on: http://localhost:5295`
4. **Terminal 2 — Web:** `dotnet run --project CityMove.Web` → espere `http://localhost:5081`
5. Abra o navegador em **http://localhost:5081**

### Logins
| Papel | E-mail | Senha |
|------|--------|-------|
| Administrador | admin@citymove.com | Admin@123 |
| Motorista | motorista1@citymove.com | Motorista@123 |
| Fiscal | fiscal@citymove.com | Fiscal@123 |
| Passageiro | passageiro@citymove.com | Passageiro@123 |

---

## 1. Visão do cidadão (parte pública — sem login)

> "O CityMove é a plataforma de transporte público da cidade. Qualquer pessoa pode consultar o sistema."

1. **Página inicial** — mostre o visual: explique em uma frase o que é o sistema (linhas, horários, paradas, clima, mapa em tempo real).
2. **Encontre sua linha** — no card de busca, escolha uma linha e clique em "Ver horários e paradas".
3. **Detalhe da linha** — mostre os **horários** por dia e as **paradas** geolocalizadas.
4. **Clima** — no menu, abra "Clima", digite uma cidade (ex.: Palmas) → dado vem da **API OpenWeatherMap** (1ª API de terceiros).
5. **Mapa ao vivo** — abra "Mapa ao vivo": os ônibus aparecem no mapa **OpenStreetMap**, atualizando a cada 5s.
6. **Acompanhar** — mostre a tabela com a posição e velocidade de cada ônibus.

**Destaque pro professor:** "Tudo isso são endpoints **públicos** da API, consumidos pelo front-end."

---

## 2. Administrador (gestão da operação)

> "Quem opera o transporte tem acesso restrito por login."

1. Clique em **Entrar** → login como **admin**.
2. **Painel** — mostre os indicadores (linhas, veículos, motoristas, viagens, etc.).
3. **Gerir linhas** — crie uma linha nova (mostra o CRUD funcionando: criar, editar, excluir).
4. **Gerir rotas** — crie/edite uma rota vinculada a uma linha.
5. **Gerir veículos** e **Gerir motoristas** — mostre rapidamente o cadastro.

**Destaque pro professor:** "CRUD completo, endpoints **autenticados** com papel **Admin** (JWT)."

---

## 3. Motorista (monitoramento GPS)

> "O motorista, em viagem, alimenta o sistema com a posição do ônibus."

1. **Sair** e entrar como **motorista1**.
2. O sistema te leva direto ao **Painel do Motorista**, mostrando a viagem em andamento (linha + veículo).
3. Clique em **"Usar meu GPS"** → autorize a localização → clique em **"Enviar posição"**.
4. (Opcional) Volte ao **Mapa ao vivo** (em outra aba, sem login) e mostre que a posição foi registrada.
5. Registre uma **ocorrência** (ex.: Atraso) para demonstrar a outra função.

**Destaque pro professor:** "Aqui está o **Monitoramento GPS** exigido no enunciado — o motorista envia, o passageiro acompanha. Regra de negócio: só registra GPS se a viagem estiver *EmAndamento*."

> Observação: a posição enviada é a sua localização real (do navegador). Em produção, quem envia é o GPS embarcado no ônibus.

---

## 4. Fiscal (fiscalização)

1. **Sair** e entrar como **fiscal**.
2. **Painel do Fiscal** — mostre a tabela da **frota** com a última posição de cada veículo.
3. **Registrar infração** — selecione motorista, veículo, tipo e descrição → registrar.

**Destaque pro professor:** "Endpoints autenticados com papel **Fiscal**."

---

## 5. Passageiro (avaliação)

1. **Sair** e entrar como **passageiro**.
2. **Painel do Passageiro** — mostre as **notificações**.
3. **Avaliar viagem** — escolha a viagem concluída, dê uma nota (1–5) e comentário → enviar.

**Destaque pro professor:** "Regra de negócio: só dá pra avaliar viagem **Concluída**, e uma avaliação por passageiro."

---

## 6. A API por trás (Swagger)

1. Abra **http://localhost:5295/swagger**
2. Mostre os endpoints separados: **públicos** (`/api/public/...`) e **autenticados** (linhas, rotas, veículos, motoristas, motorista, fiscal, passageiro, admin).
3. (Opcional) Faça um `POST /api/auth/login`, copie o **token JWT** e mostre que um endpoint protegido só responde com o token (clique em **Authorize**).

**Destaque pro professor:** "A regra de negócio fica na API; o site é só um dos consumidores — poderia ser um app ou um BI."

---

## 7. Fechamento — checklist do enunciado

Diga, ao final, que o projeto cumpre os requisitos:

- ✅ Back-end .NET (ASP.NET Core 10) + **SQL Server**
- ✅ API com endpoints **públicos** e **autenticados** (mais de 3 de cada)
- ✅ Front-end **responsivo** e com **identidade visual** própria
- ✅ Controle de versão de **código** (Git/GitHub) e de **banco** (EF Core Migrations + Seed)
- ✅ **Duas** APIs de terceiros: OpenWeatherMap (clima) e OpenStreetMap/Nominatim (geocodificação)
- ✅ **18 entidades** e os **4 papéis**: Admin, Motorista, Fiscal, Passageiro
- ✅ **Monitoramento GPS** ponta a ponta
- ✅ Regras de negócio (GPS só em viagem em andamento; avaliação só após conclusão; etc.)

---

### Dicas de apresentação
- Comece pelo **cidadão** (parte pública) — é o que prende a atenção.
- Deixe o **mapa ao vivo** aberto numa aba o tempo todo.
- Tenha os 4 logins anotados/colados num bloco de notas para trocar rápido.
- Se algo na tela vier vazio, quase sempre é a **API que não está rodando** — confira o Terminal 1.
