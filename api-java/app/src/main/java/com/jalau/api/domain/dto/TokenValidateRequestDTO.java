package com.jalau.api.domain.dto;

import jakarta.validation.constraints.NotBlank;
import lombok.Data;

@Data
public class TokenValidateRequestDTO {
    @NotBlank
    private String token;
}
