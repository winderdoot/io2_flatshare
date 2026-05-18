import { test, expect } from "@playwright/test";

const WEB_URL = "http://localhost:5173";

test.describe("registry page", () => {
    test("should register successfully as tenant", async ({
        page,
    }) => {
        await page.goto(`${WEB_URL}/create-account`);

        await page.getByText("EN", { exact: true }).click();

        const inputs = page.locator("input");

        const randomEmail =
            `test_${Date.now()}@test.com`;

        await inputs.nth(0).fill("Jan");

        await inputs.nth(1).fill("Kowalski");

        await inputs.nth(2).fill(randomEmail);

        await inputs.nth(3).fill("Password123!");

        await inputs.nth(4).fill("Password123!");

        await page.getByTestId("register-button").click();

        await expect(
            page.getByText("Your account has been registered.")
        ).toBeVisible();
    });

    test("should register successfully as landlord", async ({
        page,
    }) => {
        await page.goto(`${WEB_URL}/create-account`);

        await page.getByText("EN", { exact: true }).click();

        const inputs = page.locator("input");

        const randomEmail =
            `landlord_${Date.now()}@test.com`;

        await inputs.nth(0).fill("Adam");

        await inputs.nth(1).fill("Nowak");

        await inputs.nth(2).fill(randomEmail);

        await inputs.nth(3).fill("Password123!");

        await inputs.nth(4).fill("Password123!");

        await page.getByRole("button", {
            name: "Landlord",
        }).click();

        await page.getByTestId("register-button").click();

        await expect(
            page.getByText("Your account has been registered.")
        ).toBeVisible();
    });

    test("should show error when passwords do not match", async ({
        page,
    }) => {
        await page.goto(`${WEB_URL}/create-account`);

        await page.getByText("EN", { exact: true }).click();

        const inputs = page.locator("input");

        await inputs.nth(0).fill("Jan");

        await inputs.nth(1).fill("Kowalski");

        await inputs.nth(2).fill(
            `wrong_${Date.now()}@test.com`
        );

        await inputs.nth(3).fill("Password123!");

        await inputs.nth(4).fill("WrongPassword!");

         await page.getByTestId("register-button").click();

        await expect(
            page.getByText("Passwords do not match")
        ).toBeVisible();
    });

    test("should show email validation error", async ({
        page,
    }) => {
        await page.goto(`${WEB_URL}/create-account`);

        await page.getByText("EN", { exact: true }).click();

        const inputs = page.locator("input");

        await inputs.nth(0).fill("Jan");

        await inputs.nth(1).fill("Kowalski");

        await inputs.nth(2).fill("invalid-email");

        await inputs.nth(3).fill("Password123!");

        await inputs.nth(4).fill("Password123!");

        await page.getByTestId("register-button").click();

        await expect(
            page.getByText("Invalid email address: invalid-email")
        ).toBeVisible();
    });

    test("should redirect to login page", async ({
        page,
    }) => {
        await page.goto(`${WEB_URL}/create-account`);
        
        await page.getByText("EN", { exact: true }).click();

        await page.getByText("Sign in", {
            exact: true,
        }).click();

        await expect(page).toHaveURL(
            /\/login/
        );
    });

    test("should clear validation error after typing", async ({
        page,
    }) => {
        await page.goto(`${WEB_URL}/create-account`);
        
        await page.getByText("EN", { exact: true }).click();

        const inputs = page.locator("input");

        await inputs.nth(3).fill("Password123!");

        await inputs.nth(4).fill("WrongPassword!");

        await page.getByTestId("register-button").click();

        await expect(
            page.getByText("Passwords do not match")
        ).toBeVisible();

        await inputs.nth(4).fill("Password123!");

        await expect(
            page.getByText("Passwords do not match")
        ).not.toBeVisible();
    });

    test("should show error when password is too short", async ({
        page,
    }) => {
        await page.goto(`${WEB_URL}/create-account`);

        await page.getByText("EN", { exact: true }).click();

        const inputs = page.locator("input");

        await inputs.nth(0).fill("Jan");

        await inputs.nth(1).fill("Kowalski");

        await inputs.nth(2).fill(
            `shortpass_${Date.now()}@test.com`
        );

        await inputs.nth(3).fill("123");

        await inputs.nth(4).fill("123");

        await page.getByTestId("register-button").click();

        await expect(
            page.getByText("Password must be at least 8 characters long", { exact: true })
        ).toBeVisible();    
        
        await inputs.nth(3).fill("Password123!");

        await expect(
            page.getByText("Password must be at least 8 characters long", { exact: true })
        ).not.toBeVisible();
    });
});