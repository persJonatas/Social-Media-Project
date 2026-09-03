package com.jalau.api.security;

import com.jalau.api.domain.model.Users;
import com.jalau.api.repository.UserRepository;
import jakarta.servlet.FilterChain;
import jakarta.servlet.ServletException;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.test.util.ReflectionTestUtils;

import java.io.IOException;
import java.util.Optional;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
class SecurityFilterTest {

    @Mock
    private UserRepository userRepository;

    @Mock
    private TokenService tokenService;

    @InjectMocks
    private SecurityFilter securityFilter;

    @Mock
    private HttpServletRequest request;

    @Mock
    private HttpServletResponse response;

    @Mock
    private FilterChain filterChain;

    @BeforeEach
    void setUp() {
        SecurityContextHolder.clearContext();
        ReflectionTestUtils.setField(securityFilter, "secret", "test-secret");
    }

    @Test
    @DisplayName("doFilterInternal_noToken_continuesChain")
    void doFilterInternal_noToken_continuesChain() throws ServletException, IOException {
        when(request.getHeader("Authorization")).thenReturn(null);

        securityFilter.doFilterInternal(request, response, filterChain);

        verify(filterChain).doFilter(request, response);
        assertThat(SecurityContextHolder.getContext().getAuthentication()).isNull();
    }

    @Test
    @DisplayName("doFilterInternal_validToken_setsAuthentication")
    void doFilterInternal_validToken_setsAuthentication() throws ServletException, IOException {
        String token = "Bearer valid-token";
        when(request.getHeader("Authorization")).thenReturn(token);

        SecurityFilter spyFilter = spy(securityFilter);
        doReturn("test-login").when(spyFilter).validateToken("valid-token");

        Users user = new Users();
        user.setLogin("test-login");
        user.setPassword("pass");
        when(userRepository.findByLogin("test-login")).thenReturn(Optional.of(user));

        spyFilter.doFilterInternal(request, response, filterChain);

        verify(filterChain).doFilter(request, response);
        assertThat(SecurityContextHolder.getContext().getAuthentication()).isNotNull();
        assertThat(SecurityContextHolder.getContext().getAuthentication().getName()).isEqualTo("test-login");
    }

    @Test
    @DisplayName("doFilterInternal_invalidToken_continuesChainWithoutAuth")
    void doFilterInternal_invalidToken_continuesChainWithoutAuth() throws ServletException, IOException {
        when(request.getHeader("Authorization")).thenReturn("Bearer invalid");
        
        SecurityFilter spyFilter = spy(securityFilter);
        doReturn(null).when(spyFilter).validateToken("invalid");

        spyFilter.doFilterInternal(request, response, filterChain);

        verify(filterChain).doFilter(request, response);
        assertThat(SecurityContextHolder.getContext().getAuthentication()).isNull();
    }

    @Test
    @DisplayName("validateToken_invalidToken_returnsNull")
    void validateToken_invalidToken_returnsNull() {
        String result = securityFilter.validateToken("invalid-token");
        assertThat(result).isNull();
    }
}
