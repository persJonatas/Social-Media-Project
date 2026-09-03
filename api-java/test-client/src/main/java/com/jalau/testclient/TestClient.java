package com.jalau.testclient;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.time.Duration;

/**
 * Standalone HTTP test client that exercises the full CRUD lifecycle
 * for N users against the running API (no external dependencies).
 */
public class TestClient {

    private static final String BASE_URL = "http://localhost:8080/api/v1";
    private static final int N_USERS = 3;

    private static int passed = 0;
    private static int total = 0;

    public static void main(String[] args) throws Exception {
        HttpClient http = HttpClient.newBuilder()
                .connectTimeout(Duration.ofSeconds(5))
                .build();

        String[] userIds = new String[N_USERS];
        String[] tokens  = new String[N_USERS];
        boolean[] active = new boolean[N_USERS];

        // ----------------------------------------------------------------
        // Setup: create and login the admin observer used for VERIFY steps
        // ----------------------------------------------------------------
        String adminLogin    = "testclient_admin";
        String adminPassword = "adminpass123";
        String adminId       = null;
        String adminToken    = null;

        // 409 means admin already exists from a previous interrupted run — login proceeds normally
        HttpRequest adminCreateReq = HttpRequest.newBuilder()
                .uri(URI.create(BASE_URL + "/users"))
                .header("Content-Type", "application/json")
                .POST(HttpRequest.BodyPublishers.ofString(String.format(
                        "{\"login\":\"%s\",\"password\":\"%s\",\"name\":\"Admin Observer\"}",
                        adminLogin, adminPassword)))
                .build();
        HttpResponse<String> adminCreateRes = http.send(adminCreateReq, HttpResponse.BodyHandlers.ofString());
        if (adminCreateRes.statusCode() == 201) {
            adminId = extractString(adminCreateRes.body(), "id");
        } else if (adminCreateRes.statusCode() != 409) {
            System.out.printf("  [WARN] Admin CREATE failed (status %d) – VERIFY steps will be skipped%n",
                    adminCreateRes.statusCode());
        }

        HttpRequest adminLoginReq = HttpRequest.newBuilder()
                .uri(URI.create(BASE_URL + "/auth/login"))
                .header("Content-Type", "application/json")
                .POST(HttpRequest.BodyPublishers.ofString(String.format(
                        "{\"login\":\"%s\",\"password\":\"%s\"}", adminLogin, adminPassword)))
                .build();
        HttpResponse<String> adminLoginRes = http.send(adminLoginReq, HttpResponse.BodyHandlers.ofString());
        if (adminLoginRes.statusCode() == 200) {
            adminToken = extractString(adminLoginRes.body(), "token");
            // Resolve adminId from the JWT payload when admin pre-existed (no CREATE response available)
            if (adminId == null && adminToken != null) {
                String[] parts = adminToken.split("\\.");
                if (parts.length >= 2) {
                    String paddedPayload = parts[1];
                    int pad = paddedPayload.length() % 4;
                    if (pad != 0) paddedPayload += "=".repeat(4 - pad);
                    String payload = new String(java.util.Base64.getUrlDecoder().decode(paddedPayload));
                    adminId = extractString(payload, "userId");
                }
            }
        } else {
            System.out.printf("  [WARN] Admin LOGIN failed (status %d) – VERIFY steps will be skipped%n",
                    adminLoginRes.statusCode());
        }

        // ----------------------------------------------------------------
        // (a) CREATE
        // ----------------------------------------------------------------
        for (int i = 0; i < N_USERS; i++) {
            int n = i + 1;
            String login    = "testclient_user" + n;
            String password = "pass" + n + "123";
            String body = String.format(
                    "{\"login\":\"%s\",\"password\":\"%s\",\"name\":\"Test User %d\"}",
                    login, password, n);

            HttpRequest req = HttpRequest.newBuilder()
                    .uri(URI.create(BASE_URL + "/users"))
                    .header("Content-Type", "application/json")
                    .POST(HttpRequest.BodyPublishers.ofString(body))
                    .build();

            HttpResponse<String> res = http.send(req, HttpResponse.BodyHandlers.ofString());
            boolean ok = (res.statusCode() == 201);
            log("[CREATE]", "POST /users", login, res.statusCode(), ok ? "OK" : "FAILED");

            if (ok) {
                userIds[i] = extractString(res.body(), "id");
                active[i]  = true;
            } else {
                System.out.printf("  [WARN] CREATE failed for %s (status %d) – skipping subsequent steps%n",
                        login, res.statusCode());
            }
        }

        // ----------------------------------------------------------------
        // (b) LOGIN
        // ----------------------------------------------------------------
        for (int i = 0; i < N_USERS; i++) {
            if (!active[i]) continue;
            int n = i + 1;
            String login    = "testclient_user" + n;
            String password = "pass" + n + "123";
            String body = String.format(
                    "{\"login\":\"%s\",\"password\":\"%s\"}", login, password);

            HttpRequest req = HttpRequest.newBuilder()
                    .uri(URI.create(BASE_URL + "/auth/login"))
                    .header("Content-Type", "application/json")
                    .POST(HttpRequest.BodyPublishers.ofString(body))
                    .build();

            HttpResponse<String> res = http.send(req, HttpResponse.BodyHandlers.ofString());
            boolean ok = (res.statusCode() == 200);
            log("[LOGIN] ", "POST /auth/login", login, res.statusCode(), ok ? "OK" : "FAILED");

            if (ok) {
                tokens[i] = extractString(res.body(), "token");
                // Fall back to "accessToken" for APIs that use a different field name
                if (tokens[i] == null || tokens[i].isEmpty()) {
                    tokens[i] = extractString(res.body(), "accessToken");
                }
            } else {
                active[i] = false;
            }
        }

        // ----------------------------------------------------------------
        // (c) READ
        // ----------------------------------------------------------------
        for (int i = 0; i < N_USERS; i++) {
            if (!active[i]) continue;
            int n = i + 1;
            String login = "testclient_user" + n;
            String path  = "/users/" + userIds[i];

            HttpRequest req = HttpRequest.newBuilder()
                    .uri(URI.create(BASE_URL + path))
                    .header("Authorization", "Bearer " + tokens[i])
                    .GET()
                    .build();

            HttpResponse<String> res = http.send(req, HttpResponse.BodyHandlers.ofString());
            boolean ok = (res.statusCode() == 200);
            log("[READ]  ", "GET " + path, login, res.statusCode(), ok ? "OK" : "FAILED");
        }

        // ----------------------------------------------------------------
        // (d) UPDATE
        // ----------------------------------------------------------------
        for (int i = 0; i < N_USERS; i++) {
            if (!active[i]) continue;
            int n = i + 1;
            String login = "testclient_user" + n;
            String path  = "/users/" + userIds[i];
            String body  = String.format("{\"name\":\"Updated User %d\"}", n);

            HttpRequest req = HttpRequest.newBuilder()
                    .uri(URI.create(BASE_URL + path))
                    .header("Content-Type", "application/json")
                    .header("Authorization", "Bearer " + tokens[i])
                    .PUT(HttpRequest.BodyPublishers.ofString(body))
                    .build();

            HttpResponse<String> res = http.send(req, HttpResponse.BodyHandlers.ofString());
            boolean ok = (res.statusCode() == 200);
            log("[UPDATE]", "PUT " + path, login, res.statusCode(), ok ? "OK" : "FAILED");
        }

        // ----------------------------------------------------------------
        // (e) DELETE
        // ----------------------------------------------------------------
        for (int i = 0; i < N_USERS; i++) {
            if (!active[i]) continue;
            int n = i + 1;
            String login = "testclient_user" + n;
            String path  = "/users/" + userIds[i];

            HttpRequest req = HttpRequest.newBuilder()
                    .uri(URI.create(BASE_URL + path))
                    .header("Authorization", "Bearer " + tokens[i])
                    .DELETE()
                    .build();

            HttpResponse<String> res = http.send(req, HttpResponse.BodyHandlers.ofString());
            boolean ok = (res.statusCode() == 204 || res.statusCode() == 200);
            log("[DELETE]", "DELETE " + path, login, res.statusCode(), ok ? "OK" : "FAILED");
        }

        // ----------------------------------------------------------------
        // Verify DELETE: admin token used because the deleted users' JWTs
        // are rejected by Spring Security before reaching the controller (403)
        // ----------------------------------------------------------------
        for (int i = 0; i < N_USERS; i++) {
            if (!active[i]) continue;
            int n = i + 1;
            String login = "testclient_user" + n;
            String path  = "/users/" + userIds[i];

            if (adminToken == null) {
                System.out.printf("  [SKIP] VERIFY for %s – no admin token available%n", login);
                continue;
            }

            HttpRequest req = HttpRequest.newBuilder()
                    .uri(URI.create(BASE_URL + path))
                    .header("Authorization", "Bearer " + adminToken)
                    .GET()
                    .build();

            HttpResponse<String> res = http.send(req, HttpResponse.BodyHandlers.ofString());
            boolean ok = (res.statusCode() == 404);
            log("[VERIFY]", "GET " + path, login, res.statusCode(),
                    ok ? "404 OK (expected)" : "FAILED (expected 404, got " + res.statusCode() + ")");
        }

        // ----------------------------------------------------------------
        // Cleanup: delete the admin observer user
        // ----------------------------------------------------------------
        if (adminToken != null && adminId != null) {
            String adminPath = "/users/" + adminId;
            HttpRequest req = HttpRequest.newBuilder()
                    .uri(URI.create(BASE_URL + adminPath))
                    .header("Authorization", "Bearer " + adminToken)
                    .DELETE()
                    .build();
            HttpResponse<String> res = http.send(req, HttpResponse.BodyHandlers.ofString());
            boolean ok = (res.statusCode() == 204 || res.statusCode() == 200);
            log("[DELETE]", "DELETE " + adminPath, adminLogin, res.statusCode(), ok ? "OK" : "FAILED");
        }

        System.out.printf("%n=== RESULTS: %d/%d operations passed ===%n", passed, total);
    }

    private static void log(String tag, String endpoint, String user, int status, String outcome) {
        total++;
        if (outcome != null && (outcome.endsWith("OK") || outcome.contains("OK (expected)"))) passed++;
        System.out.printf("%-8s %s [%s] -> %d %s%n", tag, endpoint, user, status, outcome);
    }

    /**
     * Extracts a string value from a raw JSON string without an external library.
     * Handles both {@code "key":"value"} and {@code "key": "value"}.
     */
    private static String extractString(String json, String key) {
        String search = "\"" + key + "\"";
        int idx = json.indexOf(search);
        if (idx < 0) return null;
        int colon = json.indexOf(':', idx + search.length());
        if (colon < 0) return null;
        int openQuote = json.indexOf('"', colon + 1);
        if (openQuote < 0) return null;
        int closeQuote = openQuote + 1;
        while (closeQuote < json.length()) {
            if (json.charAt(closeQuote) == '"' && json.charAt(closeQuote - 1) != '\\') break;
            closeQuote++;
        }
        if (closeQuote >= json.length()) return null;
        return json.substring(openQuote + 1, closeQuote);
    }
}
