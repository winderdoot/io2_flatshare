import { test, expect } from "@playwright/test";

const WEB_URL = "http://localhost:5173";

test.describe("listings management", () => {
    test("should create new listing successfully", async ({
        page,
    }) => {
        await page.goto(`${WEB_URL}/login`);

        await page.getByText("EN", { exact: true }).click();

        const inputs = page.locator("input");

        await inputs.nth(0).fill("marek.kowalski@landlord.test");

        await inputs.nth(1).fill("LandlordPass123!");

        await page.getByTestId("login-button").click();

        await page.goto(`${WEB_URL}/my-listings/new`);
        
        const inputs2 = page.locator("input");
        const textarea = page.locator("textarea");

        await inputs2.nth(0).fill("Testowa kawalerka 2");

        await textarea.fill("Testowa kawalerka opis");

        await inputs2.nth(1).fill("123");

        await inputs2.nth(4).fill("+48 123 456 789");

        await inputs2.nth(5).fill("24");

        await inputs2.nth(6).fill("Warszawa");

        await inputs2.nth(7).fill("Wola");

        await inputs2.nth(8).fill("Wiejska");

        await inputs2.nth(9).fill("4");

        await page.getByTestId("submit-button").click();

        await expect(
            page.getByText("Listing created as Draft")
        ).toBeVisible();
    });

    test("should not be able to create new listing", async ({
        page,
    }) => {
        await page.goto(`${WEB_URL}/my-listings/new`);

        expect(page).toHaveURL(
        /\/login/
        );
    });

    test("should show error", async ({
        page,
    }) => {
        await page.goto(`${WEB_URL}/login`);

        await page.getByText("EN", { exact: true }).click();

        const inputs = page.locator("input");

        await inputs.nth(0).fill("marek.kowalski@landlord.test");

        await inputs.nth(1).fill("LandlordPass123!");

        await page.getByTestId("login-button").click();

        await page.goto(`${WEB_URL}/my-listings/new`);

        await page.getByTestId("submit-button").click();

        expect(page).toHaveURL(
        /\/my-listings\/new/
        );
    });

    test("should submit to review", async ({
        page,
    }) => {
        await page.goto(`${WEB_URL}/login`);

        await page.getByText("EN", { exact: true }).click();

        const inputs = page.locator("input");

        await inputs.nth(0).fill("marek.kowalski@landlord.test");

        await inputs.nth(1).fill("LandlordPass123!");

        await page.getByTestId("login-button").click();

        await page.goto(`${WEB_URL}/my-listings/new`);
        
        const inputs2 = page.locator("input");
        const textarea = page.locator("textarea");

        await inputs2.nth(0).fill("Testowa kawalerka");

        await textarea.fill("Testowa kawalerka opis");

        await inputs2.nth(1).fill("123");

        await inputs2.nth(4).fill("+48 123 456 789");

        await inputs2.nth(5).fill("24");

        await inputs2.nth(6).fill("Warszawa");

        await inputs2.nth(7).fill("Wola");

        await inputs2.nth(8).fill("Wiejska");

        await inputs2.nth(9).fill("4");

        await page.getByTestId("submit-button").click();

        await page.getByText("Submit for review").nth(0).click();

        await expect(
            page.getByText("Listing submitted for review")
        ).toBeVisible();
    });
});