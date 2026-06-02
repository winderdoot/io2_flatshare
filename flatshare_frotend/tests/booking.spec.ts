import { test, expect, Page } from "@playwright/test";

const WEB_URL = "http://localhost:5173";

const tenantLogIn = async (page: Page)  => {
    await page.goto(`${WEB_URL}/login`);

    await page.getByText("EN", { exact: true }).click();

    const inputs = page.locator("input");

    await inputs.nth(0).fill("ewa.nowak@tenant.test");

    await inputs.nth(1).fill("TenantPass123!");

    await page.getByTestId("login-button").click();
}

const tenantBooking = async (page: Page)  => {
        await tenantLogIn(page);

        await page.goto(`${WEB_URL}/offer`);
        
        await page.getByTestId("contact-button").nth(0).click();
        
        const inputs2 = page.locator("input");
        
        const bookingDate = new Date();
        bookingDate.setDate(bookingDate.getDate() + 7);
        
        const day = bookingDate.getDate().toString().padStart(2, "0");
        const month1 = (bookingDate.getMonth() + 1)
        .toString()
        .padStart(2, "0");
        const year = bookingDate.getFullYear();
        
        const month2 = (bookingDate.getMonth() + 2)
        .toString()
        .padStart(2, "0");
        
        const formattedDate1 = `${year}-${month1}-${day}`;
        const formattedDate2 = `${year}-${month2}-${day}`;
        
        await inputs2.nth(0).fill(formattedDate1);
        await inputs2.nth(1).fill(formattedDate2);

        await page.getByText("Book", { exact: true }).click();

        await page.goto(`${WEB_URL}/my-bookings`);

         await expect(
            page.getByText("Pending approval", { exact: true }).nth(0)
        ).toBeVisible();
};

const landlordApprove = async (page: Page)  => {
        await page.goto(`${WEB_URL}/login`);

        await page.getByText("EN", { exact: true }).click();

        const inputs = page.locator("input");

        await inputs.nth(0).fill("marek.kowalski@landlord.test");

        await inputs.nth(1).fill("LandlordPass123!");

        await page.getByTestId("login-button").click();

        await page.goto(`${WEB_URL}/booking-requests`);

        await page.getByText("Details", { exact: true }).nth(0).click();

        await page.getByText("Accept").click();
};

const tenantPay = async (page: Page)  => {
        await tenantLogIn(page);

        await page.goto(`${WEB_URL}/my-bookings`);

        await expect(
            page.getByText("Pending payment", { exact: true }).nth(0)
        ).toBeVisible();

        await page.getByText("Details", { exact: true }).nth(0).click();
        
        await expect(
            page.getByText("Pay for booking", { exact: true })
        ).toBeVisible();
}

test.describe("booking", () => {
    test("should book and be able to pay", async ({
        page,
    }) => {        
        await tenantBooking(page);
        await page.getByTestId("logout-button").click();
        await landlordApprove(page);
        await page.getByTestId("logout-button").click();
        await tenantPay(page);
    });
});