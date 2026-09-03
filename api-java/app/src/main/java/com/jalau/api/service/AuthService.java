package com.jalau.api.service;

import com.jalau.api.domain.model.Users;
import com.jalau.api.domain.dto.LoginRequestDTO;
import com.jalau.api.domain.dto.TokenResponseDTO;
import com.jalau.api.domain.dto.TokenValidateRequestDTO;
import com.jalau.api.domain.dto.TokenValidateResponseDTO;
import com.jalau.api.repository.UserRepository;
import com.jalau.api.security.TokenService;
import org.springframework.security.authentication.AuthenticationManager;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.stereotype.Service;

@Service
public class AuthService {

    private final AuthenticationManager authenticationManager;
    private final TokenService tokenService;
    private final UserRepository userRepository;

    public AuthService(AuthenticationManager authenticationManager, TokenService tokenService, UserRepository userRepository) {
        this.authenticationManager = authenticationManager;
        this.tokenService = tokenService;
        this.userRepository = userRepository;
    }

    public TokenResponseDTO login(LoginRequestDTO data) {
        var usernamePassword = new UsernamePasswordAuthenticationToken(data.getLogin(), data.getPassword());
        this.authenticationManager.authenticate(usernamePassword);

        Users user = userRepository.findByLogin(data.getLogin()).orElseThrow();
        var token = tokenService.generateToken(user);

        return new TokenResponseDTO(token);
    }

    public TokenValidateResponseDTO validateToken(TokenValidateRequestDTO data) {
        var decodedJWT = tokenService.validateTokenData(data.getToken());
        return new TokenValidateResponseDTO(
                decodedJWT.getClaim("userId").asString(),
                decodedJWT.getSubject()
        );
    }
}
