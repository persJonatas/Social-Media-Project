package com.jalau.api.service;

import com.jalau.api.domain.dto.UserRequestDTO;
import com.jalau.api.domain.dto.UserResponseDTO;
import com.jalau.api.domain.dto.UserResponse;
import com.jalau.api.domain.dto.UserUpdateDTO;
import com.jalau.api.domain.model.Users;
import com.jalau.api.exception.UserNotFoundException;
import com.jalau.api.repository.UserRepository;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.ArgumentCaptor;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;

import java.util.Collections;
import java.util.List;
import java.util.Optional;

import static org.assertj.core.api.Assertions.*;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
class UserServiceTests {

    @Mock
    private UserRepository userRepository;

    @Mock
    private BCryptPasswordEncoder encoder;

    @InjectMocks
    private UserService userService;

    private UserRequestDTO requestDTO;

    @BeforeEach
    void setUp() {
        requestDTO = new UserRequestDTO();
        requestDTO.setName("Pedro Rodrigues");
        requestDTO.setLogin("pedrodev");
        requestDTO.setPassword("pedrodev123");
    }

    // --- findAll ---

    @Test
    @DisplayName("findAll: should return list of users when users exist")
    void findAll_ShouldReturnListOfUsers_WhenUsersExist() {
        Users user1 = new Users("1", "User 1", "login1", "pass1");
        Users user2 = new Users("2", "User 2", "login2", "pass2");
        when(userRepository.findAll()).thenReturn(List.of(user1, user2));

        List<Users> result = userService.findAll();

        assertThat(result).isNotNull().hasSize(2);
        assertThat(result.get(0).getName()).isEqualTo("User 1");
    }

    @Test
    @DisplayName("findAll: should return empty list when no users exist")
    void findAll_ShouldReturnEmptyList_WhenNoUsersExist() {
        when(userRepository.findAll()).thenReturn(Collections.emptyList());

        List<Users> result = userService.findAll();

        assertThat(result).isNotNull().isEmpty();
    }

    // --- findById ---

    @Test
    @DisplayName("findById: should return user when exists")
    void findById_ShouldReturnUser_WhenUserExists() {
        Users user = new Users("123e4567", "Test User", "testlogin", "password");
        when(userRepository.findById("123e4567")).thenReturn(Optional.of(user));

        Users result = userService.findById("123e4567");

        assertThat(result).isNotNull();
        assertThat(result.getId()).isEqualTo("123e4567");
        assertThat(result.getName()).isEqualTo("Test User");
    }

    @Test
    @DisplayName("findById: should throw UserNotFoundException when user does not exist")
    void findById_ShouldThrowUserNotFoundException_WhenUserDoesNotExist() {
        when(userRepository.findById("invalid")).thenReturn(Optional.empty());

        assertThatThrownBy(() -> userService.findById("invalid"))
                .isInstanceOf(UserNotFoundException.class);
    }

    // --- createUser ---

    @Test
    @DisplayName("createUser: must create user and return DTO with id, name and login")
    void createUser_success() {
        when(userRepository.findByLogin("pedrodev")).thenReturn(Optional.empty());
        when(encoder.encode("pedrodev123")).thenReturn("hashed_password");

        Users saved = new Users();
        saved.setId("uuid-123");
        saved.setName("Pedro Rodrigues");
        saved.setLogin("pedrodev");
        saved.setPassword("hashed_password");
        when(userRepository.save(any(Users.class))).thenReturn(saved);

        UserResponseDTO response = userService.createUser(requestDTO);

        assertThat(response).isNotNull();
        assertThat(response.getId()).isEqualTo("uuid-123");
        assertThat(response.getName()).isEqualTo("Pedro Rodrigues");
        assertThat(response.getLogin()).isEqualTo("pedrodev");
    }

    @Test
    @DisplayName("createUser: the password must be stored hashed, never in plain text")
    void createUser_passwordMustBeHashed() {
        when(userRepository.findByLogin(any())).thenReturn(Optional.empty());
        when(encoder.encode("pedrodev123")).thenReturn("hashed_password");

        ArgumentCaptor<Users> captor = ArgumentCaptor.forClass(Users.class);
        when(userRepository.save(captor.capture())).thenAnswer(i -> captor.getValue());

        userService.createUser(requestDTO);

        Users persisted = captor.getValue();
        assertThat(persisted.getPassword())
                .isNotEqualTo("pedrodev123")
                .isEqualTo("hashed_password");
    }

    @Test
    @DisplayName("createUser: should throw RuntimeException when login already exists")
    void createUser_shouldThrowWhenLoginAlreadyExists() {
        Users existing = new Users();
        existing.setLogin("pedrodev");
        when(userRepository.findByLogin("pedrodev")).thenReturn(Optional.of(existing));

        assertThatThrownBy(() -> userService.createUser(requestDTO))
                .isInstanceOf(RuntimeException.class)
                .hasMessage("Login already exists");
    }

    // --- updateUser ---

    @Test
    @DisplayName("updateUser: should update and return user successfully")
    void updateUser_success() {
        Users user = new Users();
        user.setId("1");
        user.setName("Old Name");
        user.setLogin("login1");
        user.setPassword("oldHash");

        when(userRepository.findById("1")).thenReturn(Optional.of(user));
        when(userRepository.save(any())).thenReturn(user);

        UserUpdateDTO dto = new UserUpdateDTO();
        dto.setName("New Name");
        dto.setPassword("newPass123");
        when(encoder.encode("newPass123")).thenReturn("newHash");

        UserResponse result = userService.updateUser("1", dto);

        assertThat(result.getName()).isEqualTo("New Name");
        assertThat(result.getId()).isEqualTo("1");
        assertThat(result.getLogin()).isEqualTo("login1");
        assertThat(user.getPassword()).isEqualTo("newHash");
    }

    @Test
    @DisplayName("updateUser: should update name but not password when password is null")
    void updateUser_nullPassword_shouldNotUpdatePassword() {
        Users user = new Users();
        user.setId("1");
        user.setName("Old Name");
        user.setPassword("oldHash");

        when(userRepository.findById("1")).thenReturn(Optional.of(user));
        when(userRepository.save(any())).thenReturn(user);

        UserUpdateDTO dto = new UserUpdateDTO();
        dto.setName("New Name");
        dto.setPassword(null);

        userService.updateUser("1", dto);

        assertThat(user.getName()).isEqualTo("New Name");
        assertThat(user.getPassword()).isEqualTo("oldHash");
        verify(encoder, never()).encode(any());
    }

    @Test
    @DisplayName("updateUser: should throw UserNotFoundException when user does not exist")
    void updateUser_userNotFound() {
        when(userRepository.findById("999")).thenReturn(Optional.empty());

        UserUpdateDTO dto = new UserUpdateDTO();
        dto.setName("Name");
        dto.setPassword("pass123");

        assertThatThrownBy(() -> userService.updateUser("999", dto))
                .isInstanceOf(UserNotFoundException.class);
    }

    @Test
    @DisplayName("deleteUser: should delete user successfully")
    void deleteUser_success() {
        Users user = new Users();
        user.setId("1");

        when(userRepository.findById("1")).thenReturn(Optional.of(user));

        userService.deleteUser("1");

        verify(userRepository, times(1)).deleteById("1");
    }

    @Test
    @DisplayName("deleteUser: should throw UserNotFoundException when user does not exist")
    void deleteUser_userNotFound() {
        when(userRepository.findById("999")).thenReturn(Optional.empty());

        assertThatThrownBy(() -> userService.deleteUser("999"))
                .isInstanceOf(UserNotFoundException.class);
    }
}

