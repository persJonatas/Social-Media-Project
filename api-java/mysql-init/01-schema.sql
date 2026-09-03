CREATE TABLE users (
   id VARCHAR(36) PRIMARY KEY,
   name VARCHAR(200) NOT NULL,
   login VARCHAR(20) UNIQUE NOT NULL,
   password VARCHAR(100) NOT NULL
);

INSERT INTO users (id, name, login, password) VALUES
('123e4567-e89b-12d3-a456-426614174000', 'Alice', 'alice_dev',
'$2a$10$xyz...'),
('987e6543-e21b-34d3-b890-426614174000', 'Bob', 'bob_dev',
'$2a$10$xyz...');