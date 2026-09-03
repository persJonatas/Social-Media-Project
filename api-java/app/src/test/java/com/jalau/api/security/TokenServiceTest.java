package com.jalau.api.security;

import com.jalau.api.domain.model.Users;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.springframework.test.util.ReflectionTestUtils;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;

class TokenServiceTest {

    private TokenService tokenService;

    @BeforeEach
    void setUp() {
        tokenService = new TokenService();
        ReflectionTestUtils.setField(tokenService, "secret", "test-secret-key");
    }

    @Test
    @DisplayName("Should generate a valid string token for the user")
    void generateToken_success() {
        Users user = new Users();
        user.setLogin("admin");

        String token = tokenService.generateToken(user);
        assertThat(token).isNotBlank();
        
        String[] parts = token.split("\\.");
        assertThat(parts).hasSize(3);
    }

    @Test
    @DisplayName("Should successfully validate a valid token and return decoded JWT")
    void validateTokenData_success() {
        Users user = new Users();
        user.setId("u123");
        user.setLogin("admin");

        String token = tokenService.generateToken(user);
        com.auth0.jwt.interfaces.DecodedJWT decoded = tokenService.validateTokenData(token);

        assertThat(decoded).isNotNull();
        assertThat(decoded.getSubject()).isEqualTo("admin");
        assertThat(decoded.getClaim("userId").asString()).isEqualTo("u123");
    }

    @Test
    @DisplayName("Should throw exception when token is invalid")
    void validateTokenData_invalid() {
        assertThatThrownBy(() -> tokenService.validateTokenData("invalid-jwt-token"))
                .isInstanceOf(com.auth0.jwt.exceptions.JWTVerificationException.class);
    }
}
