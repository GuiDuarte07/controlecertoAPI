# Controle Certo

## Sobre

Controle Certo é um aplicativo de gerenciamento de despesas e receitas. O objetivo desse site é permitir um controle maior dos gastos do usuário, a fim de melhorar a saúde financeira dele. O diferencial do Controle Certo é sua interface amigável e de fácil compreensão. Nesse site, é possível registrar suas contas, cartões e separar suas receitas e despesas por categorias, sendo possível também ter um controle das faturas dos cartões cadastrados.

## Detalhes Técnicos

A API do sistema é desenvolvida usando .NET Framework junto com o Entity Framework Core para gerenciamento de queries do banco de dados. As rotas de autenticação, login, entre outras, foram implementadas manualmente, o que permite total liberdade para gerenciar os dados dos usuários, assim como para implementações futuras.

O projeto também tem integração com SMTP (Simple Mail Transfer Protocol) para envio de e-mails aos usuários, como confirmação de e-mail e recuperação de senha.

RabbitMQ (mensageria assíncrona) através do MassTransit: a intenção por trás do uso do RabbitMQ é, principalmente, na parte de geração de relatórios do sistema que ainda não foi desenvolvida, com o intuito de evitar uso excessivo de memória e processamento do servidor, priorizando as operações cotidianas mais essenciais, como o lançamento de transações. No momento, o RabbitMQ está sendo usado na criação de filas para envio de e-mails.

Por fim, também foi utilizado o Redis (cache em memória) para armazenamento de dados voláteis, como refresh tokens e tokens temporários (confirmação de e-mail e recuperação de senha). Ou seja, dados que têm uma validade, eliminando a necessidade de criar rotinas para apagar esses dados do meu banco de dados. Será usado futuramente também para melhorar a performance do site, mantendo alguns dados mais solicitados já preparados em memória.

Referente à parte do front-end do aplicativo, ela foi desenvolvida usando Angular 17, Tailwind CSS e Material UI. A vantagem de usar Angular é que ele dispensa a necessidade de usar bibliotecas para controlar requisições ao back-end, formulários, etc. O Angular já fornece tudo isso e muito mais de forma integrada.

Esse aplicativo foi desenvolvido utilizando tudo o que conheço sobre desenvolvimento e organização de software. Como ainda tenho muito a aprender sobre desenvolvimento, este é um projeto que será melhorado sempre que eu aprender algo novo.

## Fluxograma

O primeiro passo ao acessar o site Controle Certo é criar uma conta. Digite seu nome, e-mail e uma senha. Assim que concluir o cadastro, você já poderá fazer o login ou confirmar seu e-mail a partir de um e-mail enviado para o endereço cadastrado. Ao fazer o login, você já terá acesso a todas as funcionalidades do sistema. Para cadastrar um lançamento, é necessário primeiro ter uma conta. Então, navegue até a página de Cadastros e clique em adicionar uma conta. Caso queira também cadastrar os gastos via cartão, o caminho é Cadastros -> Cartões. A terceira opção seria a de categorias, onde você pode criar uma nova categoria ou visualizar e editar as existentes.

## Imagens
<img src="https://github.com/user-attachments/assets/a97a161c-51a7-4719-9505-b4ff953d8b4a" alt="Imagem 1" width="400"/>
<img src="https://github.com/user-attachments/assets/23e830b1-66ce-48ed-9660-4d24ff3265ae" alt="Imagem 2" width="400"/>
<img src="https://github.com/user-attachments/assets/a4a65f38-1297-4aae-a26b-11fdb17e8950" alt="Imagem 3" width="400"/>
<img src="https://github.com/user-attachments/assets/eea922e5-d67b-4d0f-97be-4890fe6453d5" alt="Imagem 4" width="400"/>

## Objetivos Futuros
Tenho a intenção de fazer deste meu projeto principal, então estarei constantemente aperfeiçoando-o. Algumas melhorias que já estão programadas para acontecer:
- Aba de relatórios de lançamentos, com filtros de conta, categorias, etc.
- Lançamentos recorrentes (diários, semanais, mensais).
- Alertas como contas a pagar, faturas encerradas, etc.
- Tradução para o inglês.
- Logs de aplicação para resolução de bugs.
- Testes automatizados.
