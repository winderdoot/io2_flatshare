import { test, expect } from "@playwright/test";

const WEB_URL = "http://localhost:5173";

test.describe("login page", () => {
    test("should show validation error", async ({
        page,
    }) => {
        await page.goto(`${WEB_URL}/login`);

        await page.getByTestId("login-button").click();

        await expect(
        page.locator(".error-message")
        ).toContainText("Fill all fields");
    });

    test("should login successfully", async ({
        page,
    }) => {
        await page.goto(`${WEB_URL}/login`);

        const inputs = page.locator("input");

        await inputs.nth(0).fill("ewa.nowak@tenant.test");

        await inputs.nth(1).fill("TenantPass123!");

        await page.getByTestId("login-button").click();

        await expect(page).not.toHaveURL(
        /\/login/
        );
    });

    test("should show error for invalid credentials", async ({
        page,
    }) => {
        await page.goto(`${WEB_URL}/login`);

        await page.getByText("EN", { exact: true }).click();

        const inputs = page.locator("input");

        await inputs.nth(0).fill("wrong@test.com");

        await inputs.nth(1).fill("wrongpassword");

        await page.getByTestId("login-button").click();

        await expect(
        page.locator(".error-message")
        ).toContainText(
        "Invalid email or password"
        );
    });
});