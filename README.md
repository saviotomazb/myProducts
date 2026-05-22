# MyProducts

![GitHub repo size](https://img.shields.io/github/repo-size/saviotomazb/myProducts?style=for-the-badge)
![GitHub language count](https://img.shields.io/github/languages/count/saviotomazb/myProducts?style=for-the-badge)
![GitHub forks](https://img.shields.io/github/forks/saviotomazb/myProducts?style=for-the-badge)
![GitHub open issues](https://img.shields.io/github/issues/saviotomazb/myProducts?style=for-the-badge)
![GitHub pull requests](https://img.shields.io/github/issues-pr/saviotomazb/myProducts?style=for-the-badge)

<img src="wwwroot/images/Logotipo_branco.png" alt="Logotipo do MyProducts">

> O MyProducts é uma aplicação web para cadastro de produtos, categorias, clientes e geração de orçamentos. O sistema também conta com autenticação de usuários, recuperação de senha por e-mail, registros de logs e uma área de dashboard para acompanhar dados operacionais.

### Ajustes e melhorias

O projeto ainda está em desenvolvimento e as próximas atualizações serão voltadas para as seguintes tarefas:

- [x] Cadastro e autenticação de usuários
- [x] Cadastro, listagem e edicao de categorias
- [x] Cadastro, listagem e edicao de produtos
- [x] Cadastro, listagem e edicao de clientes
- [x] Criacao e listagem de orçamentos
- [ ] Evolução dos gráficos e indicadores do dashboard
- [ ] Criação de testes automatizados
- [ ] Documentação de deploy em ambiente de produção

## 💻 Pré-requisitos

Antes de começar, verifique se você atendeu aos seguintes requisitos:

- Você instalou o `.NET 9 SDK`.
- Você instalou o `Node.js` e o `npm` para compilar os arquivos do Tailwind CSS.
- Você tem acesso a uma instância do `SQL Server`.
- Você tem uma máquina `Windows`, `Linux` ou `macOS` com suporte ao .NET 9.
- Você configurou as variáveis de ambiente usadas pela aplicação.

Variáveis de ambiente necessárias:

```env
DefaultConnection=Server=SEU_SERVIDOR;Database=MYPRODUCTS;Trusted_Connection=True;TrustServerCertificate=True
Jwt:Key=SUA_CHAVE_SECRETA_PARA_ASSINATURA_DO_JWT
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=seuemail@gmail.com
SMTP_PASS=sua_senha_ou_app_password
```

## 🚀 Instalando MyProducts

Para instalar o MyProducts, siga estas etapas:

Linux e macOS:

```bash
git clone https://github.com/saviotomazb/myProducts.git
cd myProducts
dotnet restore
npm install
dotnet ef database update
```

Windows:

```powershell
git clone https://github.com/saviotomazb/myProducts.git
cd myProducts
dotnet restore
npm install
dotnet ef database update
```

O comando `dotnet ef database update` cria ou atualiza o banco de dados configurado em `DefaultConnection`.

## ☕ Usando MyProducts

Para usar o MyProducts, siga estas etapas:

```bash
npm run dev
```

O comando acima executa a aplicação com `dotnet watch run` e compila o CSS com Tailwind em modo observação. Após iniciar, acesse a URL informada pelo ASP.NET Core no terminal.

Principais áreas do sistema:

- `Login` e `Cadastro`: autenticação de usuários e criação de contas.
- `Recuperacao de senha`: envio de código por e-mail via SMTP.
- `Produtos`: cadastro, busca, paginação e edição de produtos.
- `Categorias`: cadastro, busca, paginação e edição de categorias.
- `Clientes`: cadastro, busca, paginação e edição de clientes.
- `Orcamentos`: criação de orçamentos com itens, quantidades, validade, cliente e valor total.
- `Dashboard`: área reservada para visualização de indicadores operacionais.

## 🛠️ Tecnologias utilizadas

- ASP.NET Core Razor Pages
- Entity Framework Core
- SQL Server
- Tailwind CSS
- JWT Bearer Authentication
- Serilog
- SMTP para recuperacao de senha

## 📫 Contribuindo para MyProducts

Para contribuir com MyProducts, siga estas etapas:

1. Bifurque este repositorio.
2. Crie um branch: `git checkout -b minha-funcionalidade`.
3. Faça suas alterações e confirme-as: `git commit -m "feat: minha nova funcionalidade"`.
4. Envie para o branch original: `git push origin minha-funcionalidade`.
5. Crie a solicitação de pull.

Como alternativa, consulte a documentação do GitHub em [como criar uma solicitacao pull](https://help.github.com/en/github/collaborating-with-issues-and-pull-requests/creating-a-pull-request).

## 🤝 Colaboradores

Agradecemos as seguintes pessoas que contribuiram para este projeto:

<table>
  <tr>
    <td align="center">
      <a href="https://github.com/saviotomazb" title="Perfil de Sávio Tomaz no GitHub">
        <img src="https://avatars.githubusercontent.com/saviotomazb" width="100px;" alt="Foto de Sávio Tomaz no GitHub"/><br>
        <sub>
          <b>Sávio Tomaz</b>
        </sub>
      </a>
    </td>
  </tr>
</table>

## 😄 Seja um dos contribuidores

Quer fazer parte desse projeto? Abra uma issue ou envie uma pull request com melhorias, correções ou novas funcionalidades.

## 📝 Licença

Esse projeto esta sob licença ISC. Veja o arquivo [package.json](package.json) para mais detalhes.
