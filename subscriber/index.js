import EventSource from "eventsource";
import jwt from "jsonwebtoken";
import dotenv from "dotenv";

// Carregar as variáveis de ambiente
dotenv.config({
    path: "../.env",
});

// Chave secreta para assinar o token
const chaveSecreta = process.env.JWT_SECRET;

// Dados do payload (informações sobre o usuário)
const payload = {
  usuario_id: '123456',
  email: 'usuario@example.com',
  mercure: {
    subscribe: ['https://example.com/books/{id}'], // Topicos que o cliente pode escutar
  },
};

// Opções do token
const options = {
  expiresIn: '1h', // Tempo de expiração do token (1 hora)
  issuer: 'sua_issuer_aqui',
  audience: 'sua_audience_aqui',
};

// Gerar o token JWT
const token = jwt.sign(payload, chaveSecreta, options);

const url = new URL("http://localhost:8090/.well-known/mercure");
url.searchParams.append("topic", "https://example.com/books/{id}");

var eventSource = new EventSource(url.toString(), {
  withCredentials: true,
  headers: {
    Authorization: `Bearer ${token}`
  },
});

eventSource.onmessage = (e) => console.log(`Mensagem: ${e.data}`);
eventSource.onerror = (e) => console.error(e);
