package com.jalau.api.service;

import java.util.List;
import java.util.UUID;

import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;
import org.springframework.stereotype.Service;

import com.jalau.api.domain.dto.UserRequestDTO;
import com.jalau.api.domain.dto.UserResponseDTO;
import com.jalau.api.domain.dto.UserResponse;
import com.jalau.api.domain.dto.UserUpdateDTO;
import com.jalau.api.domain.model.Users;
import com.jalau.api.exception.UserNotFoundException;
import com.jalau.api.repository.UserRepository;

import lombok.RequiredArgsConstructor;

@Service
@RequiredArgsConstructor
public class UserService {
    private final UserRepository userRepository;
    private final BCryptPasswordEncoder encoder;

    public List<Users> findAll() {
        return userRepository.findAll();
    }

    public Users findById(String id) {
        return userRepository.findById(id)
                .orElseThrow(() -> new UserNotFoundException("User not found"));
    }

    public UserResponseDTO createUser(UserRequestDTO requestDTO) {
        userRepository.findByLogin(requestDTO.getLogin())
                .ifPresent(users -> {
                    throw new RuntimeException("Login already exists");
                });

        Users user = new Users();
        user.setId(UUID.randomUUID().toString());
        user.setName(requestDTO.getName());
        user.setLogin(requestDTO.getLogin());
        user.setPassword(encoder.encode(requestDTO.getPassword()));

        Users saved = userRepository.save(user);

        return new UserResponseDTO(saved);
    }

    public UserResponse updateUser(String id, UserUpdateDTO dto) {
        Users user = userRepository.findById(id)
                .orElseThrow(() -> new UserNotFoundException("User not found"));

        user.setName(dto.getName());

        if (dto.getPassword() != null && !dto.getPassword().isBlank()) {
            user.setPassword(encoder.encode(dto.getPassword()));
        }

        userRepository.save(user);
        return toResponse(user);
    }

    public void deleteUser(String id) {
        userRepository.findById(id)
                .orElseThrow(() -> new UserNotFoundException("User not found"));

        userRepository.deleteById(id);
    }

    private UserResponse toResponse(Users user) {
        UserResponse response = new UserResponse();
        response.setId(user.getId());
        response.setName(user.getName());
        response.setLogin(user.getLogin());
        return response;
    }
}
