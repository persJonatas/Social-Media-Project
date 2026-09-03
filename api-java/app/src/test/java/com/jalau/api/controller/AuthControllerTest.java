package com.jalau.api.controller;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.jalau.api.domain.dto.LoginRequestDTO;
import com.jalau.api.domain.dto.TokenResponseDTO;
import com.jalau.api.domain.dto.TokenValidateRequestDTO;
import com.jalau.api.domain.dto.TokenValidateResponseDTO;
import com.jalau.api.service.AuthService;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.autoconfigure.web.servlet.WebMvcTest;
import org.springframework.boot.test.mock.mockito.MockBean;
import org.springframework.http.MediaType;
import org.springframework.security.authentication.BadCredentialsException;
import org.springframework.test.web.servlet.MockMvc;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@WebMvcTest(AuthController.class)
@AutoConfigureMockMvc(addFilters = false)
class AuthControllerTest {

    @Autowired
    private MockMvc mockMvc;

    @MockBean
    private AuthService authService;

    @MockBean
    private com.jalau.api.security.SecurityFilter securityFilter;

    @Autowired
    private ObjectMapper objectMapper;

    @Test
    @DisplayName("login_success_returns200")
    void login_success() throws Exception {
        LoginRequestDTO request = new LoginRequestDTO();
        request.setLogin("testUser");
        request.setPassword("password123");

        TokenResponseDTO tokenResp = new TokenResponseDTO("mocked-jwt-token");
        when(authService.login(any(LoginRequestDTO.class))).thenReturn(tokenResp);

        mockMvc.perform(post("/api/v1/auth/login")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(request)))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.token").value("mocked-jwt-token"));
    }

    @Test
    @DisplayName("login_invalidCredentials_returns401")
    void login_invalidCredentials() throws Exception {
        LoginRequestDTO request = new LoginRequestDTO();
        request.setLogin("wrongUser");
        request.setPassword("wrongPass");

        when(authService.login(any(LoginRequestDTO.class)))
                .thenThrow(new BadCredentialsException("Bad credentials"));

        mockMvc.perform(post("/api/v1/auth/login")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(request)))
                .andExpect(status().isUnauthorized())
                .andExpect(jsonPath("$.message").value("Invalid login or password"));
    }

    @Test
    @DisplayName("login_validationError_returns422")
    void login_validationError() throws Exception {
        LoginRequestDTO request = new LoginRequestDTO();
        request.setLogin("testUser");

        mockMvc.perform(post("/api/v1/auth/login")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(request)))
                .andExpect(status().isUnprocessableEntity());
    }

    @Test
    @DisplayName("validate_success_returns200")
    void validate_success() throws Exception {
        TokenValidateRequestDTO validateRequest = new TokenValidateRequestDTO();
        validateRequest.setToken("valid-token");

        TokenValidateResponseDTO validateResp = new TokenValidateResponseDTO("user123", "testUser");
        when(authService.validateToken(any(TokenValidateRequestDTO.class))).thenReturn(validateResp);

        mockMvc.perform(post("/api/v1/auth/validate")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(validateRequest)))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.userId").value("user123"))
                .andExpect(jsonPath("$.login").value("testUser"));
    }

    @Test
    @DisplayName("validate_invalidToken_returns401")
    void validate_invalidToken() throws Exception {
        TokenValidateRequestDTO validateRequest = new TokenValidateRequestDTO();
        validateRequest.setToken("invalid-token");

        when(authService.validateToken(any(TokenValidateRequestDTO.class)))
                .thenThrow(new com.auth0.jwt.exceptions.JWTVerificationException("Invalid signature"));

        mockMvc.perform(post("/api/v1/auth/validate")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(validateRequest)))
                .andExpect(status().isUnauthorized())
                .andExpect(jsonPath("$.message").value("Invalid or expired token"));
    }
}
