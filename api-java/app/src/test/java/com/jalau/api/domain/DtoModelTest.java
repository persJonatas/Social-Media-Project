package com.jalau.api.domain;

import com.jalau.api.domain.dto.*;
import com.jalau.api.domain.model.Users;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import java.time.LocalDateTime;

import static org.assertj.core.api.Assertions.assertThat;

class DtoModelTest {

    @Test
    @DisplayName("Users_model_coverage")
    void users_model_coverage() {
        Users user = new Users("1", "Name", "login", "pass");
        assertThat(user.getId()).isEqualTo("1");
        assertThat(user.getName()).isEqualTo("Name");
        assertThat(user.getLogin()).isEqualTo("login");
        assertThat(user.getPassword()).isEqualTo("pass");

        user.setId("2");
        user.setName("Name2");
        user.setLogin("login2");
        user.setPassword("pass2");
        assertThat(user.getId()).isEqualTo("2");
        assertThat(user.getName()).isEqualTo("Name2");
        assertThat(user.getLogin()).isEqualTo("login2");
        assertThat(user.getPassword()).isEqualTo("pass2");

        Users empty = new Users();
        assertThat(empty).isNotNull();
    }

    @Test
    @DisplayName("UserResponseDTO_constructor_coverage")
    void userResponseDTO_constructor_coverage() {
        Users user = new Users("1", "Name", "login", "pass");
        UserResponseDTO dto = new UserResponseDTO(user);
        assertThat(dto.getId()).isEqualTo("1");
        assertThat(dto.getName()).isEqualTo("Name");
        assertThat(dto.getLogin()).isEqualTo("login");

        UserResponseDTO empty = new UserResponseDTO();
        empty.setId("id");
        assertThat(empty.getId()).isEqualTo("id");
    }

    @Test
    @DisplayName("LoginRequestDTO_coverage")
    void loginRequestDTO_coverage() {
        LoginRequestDTO dto = new LoginRequestDTO();
        dto.setLogin("l");
        dto.setPassword("p");
        assertThat(dto.getLogin()).isEqualTo("l");
        assertThat(dto.getPassword()).isEqualTo("p");
    }

    @Test
    @DisplayName("TokenResponseDTO_coverage")
    void tokenResponseDTO_coverage() {
        TokenResponseDTO dto = new TokenResponseDTO("t");
        assertThat(dto.getToken()).isEqualTo("t");
        dto.setToken("t2");
        assertThat(dto.getToken()).isEqualTo("t2");
    }

    @Test
    @DisplayName("TokenValidateRequestDTO_coverage")
    void tokenValidateRequestDTO_coverage() {
        TokenValidateRequestDTO dto = new TokenValidateRequestDTO();
        dto.setToken("t");
        assertThat(dto.getToken()).isEqualTo("t");
    }

    @Test
    @DisplayName("TokenValidateResponseDTO_coverage")
    void tokenValidateResponseDTO_coverage() {
        TokenValidateResponseDTO dto = new TokenValidateResponseDTO("u", "l");
        assertThat(dto.getUserId()).isEqualTo("u");
        assertThat(dto.getLogin()).isEqualTo("l");
    }

    @Test
    @DisplayName("ErrorResponseDTO_coverage")
    void errorResponseDTO_coverage() {
        LocalDateTime now = LocalDateTime.now();
        ErrorResponseDTO dto = new ErrorResponseDTO(400, "Bad", "Msg", now);
        assertThat(dto.getStatus()).isEqualTo(400);
        assertThat(dto.getError()).isEqualTo("Bad");
        assertThat(dto.getMessage()).isEqualTo("Msg");
        assertThat(dto.getTimestamp()).isEqualTo(now);
    }

    @Test
    @DisplayName("UserRequestDTO_coverage")
    void userRequestDTO_coverage() {
        UserRequestDTO dto = new UserRequestDTO();
        dto.setName("n");
        dto.setLogin("l");
        dto.setPassword("p");
        assertThat(dto.getName()).isEqualTo("n");
        assertThat(dto.getLogin()).isEqualTo("l");
        assertThat(dto.getPassword()).isEqualTo("p");
    }

    @Test
    @DisplayName("UserResponse_coverage")
    void userResponse_coverage() {
        UserResponse dto = new UserResponse();
        dto.setId("i");
        dto.setName("n");
        dto.setLogin("l");
        assertThat(dto.getId()).isEqualTo("i");
        assertThat(dto.getName()).isEqualTo("n");
        assertThat(dto.getLogin()).isEqualTo("l");
    }

    @Test
    @DisplayName("UserUpdateDTO_coverage")
    void userUpdateDTO_coverage() {
        UserUpdateDTO dto = new UserUpdateDTO();
        dto.setName("n");
        dto.setPassword("p");
        assertThat(dto.getName()).isEqualTo("n");
        assertThat(dto.getPassword()).isEqualTo("p");
    }
}
