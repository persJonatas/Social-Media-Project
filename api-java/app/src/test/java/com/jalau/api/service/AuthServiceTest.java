package com.jalau.api.service;

import com.jalau.api.domain.dto.LoginRequestDTO;
import com.jalau.api.domain.dto.TokenResponseDTO;
import com.jalau.api.domain.dto.TokenValidateRequestDTO;
import com.jalau.api.domain.dto.TokenValidateResponseDTO;
import com.jalau.api.domain.model.Users;
import com.jalau.api.repository.UserRepository;
import com.jalau.api.security.TokenService;
import com.auth0.jwt.interfaces.DecodedJWT;
import com.auth0.jwt.interfaces.Claim;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.security.authentication.AuthenticationManager;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;

import java.util.Optional;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
class AuthServiceTest {

    @Mock
    private AuthenticationManager authenticationManager;

    @Mock
    private TokenService tokenService;

    @Mock
    private UserRepository userRepository;

    @InjectMocks
    private AuthService authService;

    @Test
    @DisplayName("login_validCredentials_returnsToken")
    void login_validCredentials_returnsToken() {
        LoginRequestDTO request = new LoginRequestDTO();
        request.setLogin("test");
        request.setPassword("pass");

        Users user = new Users();
        user.setLogin("test");

        when(userRepository.findByLogin("test")).thenReturn(Optional.of(user));
        when(tokenService.generateToken(user)).thenReturn("mock-token");

        TokenResponseDTO response = authService.login(request);

        assertThat(response.getToken()).isEqualTo("mock-token");
        verify(authenticationManager).authenticate(any(UsernamePasswordAuthenticationToken.class));
    }

    @Test
    @DisplayName("login_userNotFound_throwsException")
    void login_userNotFound_throwsException() {
        LoginRequestDTO request = new LoginRequestDTO();
        request.setLogin("test");
        request.setPassword("pass");

        when(userRepository.findByLogin("test")).thenReturn(Optional.empty());

        assertThatThrownBy(() -> authService.login(request))
                .isInstanceOf(java.util.NoSuchElementException.class);
    }

    @Test
    @DisplayName("validateToken_validToken_returnsData")
    void validateToken_validToken_returnsData() {
        TokenValidateRequestDTO request = new TokenValidateRequestDTO();
        request.setToken("valid-token");

        DecodedJWT decodedJWT = mock(DecodedJWT.class);
        Claim userIdClaim = mock(Claim.class);
        when(userIdClaim.asString()).thenReturn("user-id");
        when(decodedJWT.getClaim("userId")).thenReturn(userIdClaim);
        when(decodedJWT.getSubject()).thenReturn("test-user");
        when(tokenService.validateTokenData("valid-token")).thenReturn(decodedJWT);

        TokenValidateResponseDTO response = authService.validateToken(request);

        assertThat(response.getUserId()).isEqualTo("user-id");
        assertThat(response.getLogin()).isEqualTo("test-user");
    }
}
