package com.jalau.api.controller;

import com.jalau.api.domain.dto.LoginRequestDTO;
import com.jalau.api.domain.dto.TokenResponseDTO;
import com.jalau.api.domain.dto.TokenValidateRequestDTO;
import com.jalau.api.domain.dto.TokenValidateResponseDTO;
import com.jalau.api.service.AuthService;
import jakarta.validation.Valid;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/v1/auth")
public class AuthController {

    private final AuthService authService;

    public AuthController(AuthService authService) {
        this.authService = authService;
    }

    @PostMapping("/login")
    public ResponseEntity<TokenResponseDTO> login(@RequestBody @Valid LoginRequestDTO data) {
        return ResponseEntity.ok(authService.login(data));
    }

    @PostMapping("/validate")
    public ResponseEntity<TokenValidateResponseDTO> validateToken(@RequestBody @Valid TokenValidateRequestDTO data) {
        return ResponseEntity.ok(authService.validateToken(data));
    }
}
