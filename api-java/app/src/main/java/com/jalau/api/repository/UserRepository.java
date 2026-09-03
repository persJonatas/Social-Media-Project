package com.jalau.api.repository;

import com.jalau.api.domain.model.Users;
import org.springframework.data.mongodb.repository.MongoRepository;

import java.util.Optional;

public interface UserRepository extends MongoRepository<Users, String> {
        Optional<Users> findByLogin(String login);
}