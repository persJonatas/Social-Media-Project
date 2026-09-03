package com.jalau.api.domain.dto;

import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.Size;
import lombok.Data;

@Data
public class UserUpdateDTO {

    @NotBlank(message = "Name must not be blank")
    private String name;

    @Size(min = 6, message = "Password must be at least 6 characters")
    private String password;
}
