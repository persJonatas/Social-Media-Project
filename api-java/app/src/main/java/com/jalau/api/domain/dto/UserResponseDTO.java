package com.jalau.api.domain.dto;

import com.jalau.api.domain.model.Users;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class UserResponseDTO {
    private String id;
    private String name;
    private String login;

    public UserResponseDTO(Users user) {
        this.id = user.getId();
        this.name = user.getName();
        this.login = user.getLogin();
    }

}
