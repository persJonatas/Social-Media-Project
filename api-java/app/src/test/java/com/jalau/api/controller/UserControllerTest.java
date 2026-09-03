package com.jalau.api.controller;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.jalau.api.domain.dto.UserRequestDTO;
import com.jalau.api.domain.dto.UserResponseDTO;
import com.jalau.api.domain.dto.UserResponse;
import com.jalau.api.domain.dto.UserUpdateDTO;
import com.jalau.api.domain.model.Users;
import com.jalau.api.exception.UserNotFoundException;
import com.jalau.api.service.UserService;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.autoconfigure.web.servlet.WebMvcTest;
import org.springframework.boot.test.mock.mockito.MockBean;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;

import java.util.List;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.Mockito.*;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.*;

@WebMvcTest(UserController.class)
@AutoConfigureMockMvc(addFilters = false)
class UserControllerTest {

    @Autowired
    private MockMvc mockMvc;

    @MockBean
    private UserService userService;

    @MockBean
    private com.jalau.api.security.SecurityFilter securityFilter;

    @Autowired
    private ObjectMapper objectMapper;

    private UserRequestDTO requestDTO;
    private UserResponseDTO responseDTO;
    private Users user1;

    @BeforeEach
    void setUp() {
        requestDTO = new UserRequestDTO();
        requestDTO.setName("Pedro Rodrigues");
        requestDTO.setLogin("pedrodev");
        requestDTO.setPassword("pedrodev123");

        responseDTO = new UserResponseDTO();
        responseDTO.setId("uuid-123");
        responseDTO.setName("Pedro Rodrigues");
        responseDTO.setLogin("pedrodev");

        user1 = new Users("1", "Alice", "alice_dev", "secret");
    }

    @Test
    @DisplayName("listAll_success_returns200")
    void listAll_success() throws Exception {
        when(userService.findAll()).thenReturn(List.of(user1));

        mockMvc.perform(get("/api/v1/users"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$[0].name").value("Alice"));
    }

    @Test
    @DisplayName("getUserById_exists_returns200")
    void getUserById_exists() throws Exception {
        when(userService.findById("1")).thenReturn(user1);

        mockMvc.perform(get("/api/v1/users/1"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.id").value("1"));
    }

    @Test
    @DisplayName("getUserById_notFound_returns404")
    void getUserById_notFound() throws Exception {
        when(userService.findById("999")).thenThrow(new UserNotFoundException("User not found"));

        mockMvc.perform(get("/api/v1/users/999"))
                .andExpect(status().isNotFound())
                .andExpect(jsonPath("$.message").value("User not found"));
    }

    @Test
    @DisplayName("createUser_success_returns201")
    void createUser_success() throws Exception {
        when(userService.createUser(any(UserRequestDTO.class))).thenReturn(responseDTO);

        mockMvc.perform(post("/api/v1/users")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(requestDTO)))
                .andExpect(status().isCreated())
                .andExpect(jsonPath("$.id").value("uuid-123"));
    }

    @Test
    @DisplayName("updateUser_success_returns200")
    void updateUser_success() throws Exception {
        UserUpdateDTO updateDTO = new UserUpdateDTO();
        updateDTO.setName("Updated Name");
        updateDTO.setPassword("newPass");

        UserResponse userResponse = new UserResponse();
        userResponse.setId("1");
        userResponse.setName("Updated Name");
        userResponse.setLogin("alice_dev");

        when(userService.updateUser(eq("1"), any(UserUpdateDTO.class))).thenReturn(userResponse);

        mockMvc.perform(put("/api/v1/users/1")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(updateDTO)))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.name").value("Updated Name"));
    }

    @Test
    @DisplayName("deleteUser_success_returns204")
    void deleteUser_success() throws Exception {
        doNothing().when(userService).deleteUser("1");

        mockMvc.perform(delete("/api/v1/users/1"))
                .andExpect(status().isNoContent());

        verify(userService).deleteUser("1");
    }
}
