package com.jalau.api.exception;

import com.jalau.api.domain.dto.ErrorResponseDTO;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.security.authentication.BadCredentialsException;
import org.springframework.validation.BindingResult;
import org.springframework.validation.FieldError;
import org.springframework.web.bind.MethodArgumentNotValidException;

import java.util.List;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

class GlobalExceptionHandlerTest {

    private final GlobalExceptionHandler handler = new GlobalExceptionHandler();

    @Test
    @DisplayName("handleUserNotFoundException_returns404")
    void handleUserNotFoundException_returns404() {
        UserNotFoundException ex = new UserNotFoundException("Not found");
        ResponseEntity<ErrorResponseDTO> response = handler.handleUserNotFoundException(ex);

        assertThat(response.getStatusCode()).isEqualTo(HttpStatus.NOT_FOUND);
        assertThat(response.getBody().getMessage()).isEqualTo("Not found");
    }

    @Test
    @DisplayName("handleValidationExceptions_returns422")
    void handleValidationExceptions_returns422() {
        MethodArgumentNotValidException ex = mock(MethodArgumentNotValidException.class);
        BindingResult bindingResult = mock(BindingResult.class);
        FieldError fieldError = new FieldError("obj", "field", "default message");
        
        when(ex.getBindingResult()).thenReturn(bindingResult);
        when(bindingResult.getFieldErrors()).thenReturn(List.of(fieldError));

        ResponseEntity<ErrorResponseDTO> response = handler.handleValidationExceptions(ex);

        assertThat(response.getStatusCode()).isEqualTo(HttpStatus.UNPROCESSABLE_ENTITY);
        assertThat(response.getBody().getMessage()).contains("field: default message");
    }

    @Test
    @DisplayName("handleBadCredentialsException_returns401")
    void handleBadCredentialsException_returns401() {
        BadCredentialsException ex = new BadCredentialsException("Bad");
        ResponseEntity<ErrorResponseDTO> response = handler.handleBadCredentialsException(ex);

        assertThat(response.getStatusCode()).isEqualTo(HttpStatus.UNAUTHORIZED);
        assertThat(response.getBody().getMessage()).isEqualTo("Invalid login or password");
    }

    @Test
    @DisplayName("handleJWTVerificationException_returns401")
    void handleJWTVerificationException_returns401() {
        com.auth0.jwt.exceptions.JWTVerificationException ex = new com.auth0.jwt.exceptions.JWTVerificationException("Expired");
        ResponseEntity<ErrorResponseDTO> response = handler.handleJWTVerificationException(ex);

        assertThat(response.getStatusCode()).isEqualTo(HttpStatus.UNAUTHORIZED);
        assertThat(response.getBody().getMessage()).isEqualTo("Invalid or expired token");
    }

    @Test
    @DisplayName("handleRuntimeException_returns409")
    void handleRuntimeException_returns409() {
        RuntimeException ex = new RuntimeException("Conflict");
        ResponseEntity<ErrorResponseDTO> response = handler.handleRuntimeException(ex);

        assertThat(response.getStatusCode()).isEqualTo(HttpStatus.CONFLICT);
        assertThat(response.getBody().getMessage()).isEqualTo("Conflict");
    }

    @Test
    @DisplayName("handleGenericException_returns500")
    void handleGenericException_returns500() {
        Exception ex = new Exception("Generic");
        ResponseEntity<ErrorResponseDTO> response = handler.handleGenericException(ex);

        assertThat(response.getStatusCode()).isEqualTo(HttpStatus.INTERNAL_SERVER_ERROR);
        assertThat(response.getBody().getMessage()).isEqualTo("An unexpected error occurred");
    }
}
