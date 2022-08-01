# My Movie Score


# Resumo do projeto

Este projeto tem como intuito, desenvolver uma aplicação onde o usuario possa se registar e salvar filmes em sua lista, para que possa avaliar POSTeriormente.

Tecnologias empregadas nessa aplicação foram:

- .NET Core 6.0
- c#
- Ef Core
- SQL Server
- MediatR
- CQRS
- Fluent Validation
- Padrão Repository

## 🔨 Funcionalidades do projeto

- `Funcionalidade 1` `Cadastro de usuario`: Possui a responsabilidade de criar um usuario e criptografar sua senha.

**Para criar um usuario:**
```
POST
/api/user

{
  "email": "string",
  "password": "string",
  "name": "string"
}
```

- `Funcionalidade 2` `Consultar de usuario por id`: Possui a responsabilidade de consultar o usuario por id.

**Para consultar um usuario:**
```
GET
/api/user/{id}
{
  "email": "string",
  "password": "string",
  "name": "string"
}
```
- `Funcionalidade 3` `Login`: Possui a responsabilidade autenticar os dados informados e gerar um bearer token com JWT para que o usuario possa utilizar nas requisições em relação ao filme.

**Para logar um usuario:**
```
POST
/api/user/login

{
  "email": "string",
  "password": "string",
}
```
- `Funcionalidade 4` `Cadastar filmes`: Possui a responsabilidade cadastar os filmes que o usuario pretende avaliar.

**Para cadastrar um filme:**
```
POST
/api/user/movie

{
  "userId": 0,
  "idIMDb": "string",
  "watched": true,
  "userScore": 0
}
```
- `Funcionalidade 5` `consultar filmes`: Possui a responsabilidade de consultar os filmes cadastrados.

**Para consultar os filmes:**
```
GET
/api/user/movie
```
- `Funcionalidade 6` `consultar filme por id`: Possui a responsabilidade de consultar um filme especifico pelo seu ID.

**Para consultar os filmes:**
```
GET
/api/user/movie/{id}
```
- `Funcionalidade 7` `Atualizar filme por id`: Possui a responsabilidade de atualizar um filme especifico pelo seu ID.

**Para atualizar um filme:**
```
PUT
/api/user/movie
```
- `Funcionalidade 7` `Deletar filme por id`: Possui a responsabilidade de deletar um filme especifico pelo seu ID.

**Para deletar um filme por id:**
```
DELETE
/api/user/movie
```

# Utilização
Para melhor utilização deste projeto recomenda-se a utilização do docker que ira instalar os containers com as dependencias necessarias, para isso instale o [docker](https://docs.docker.com/desktop/install/windows-install/) (pode ser necessario a instalação do WSL2 para windows).
Após isso utilizar o comando:

```
docker compose up -d
```

Para melhor visualizar os exemplos das funcionalidades acima, importe o arquivo(MyMovieScore.postman_collection) no postman.



