import { test, expect } from '@playwright/test';

test.describe('Task management', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('shows the task list page', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'TaskFlow AI' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Tasks' })).toBeVisible();
  });

  test('creates a new task', async ({ page }) => {
    await page.getByLabel('Task title').fill('E2E task created by Playwright');
    await page.getByRole('button', { name: 'Add Task' }).click();
    await expect(page.getByText('E2E task created by Playwright')).toBeVisible();
  });

  test('deletes a task', async ({ page }) => {
    // Create first
    await page.getByLabel('Task title').fill('Task to delete');
    await page.getByRole('button', { name: 'Add Task' }).click();
    const task = page.getByText('Task to delete');
    await expect(task).toBeVisible();

    // Delete
    await page.getByRole('button', { name: /delete task task to delete/i }).click();
    await expect(task).not.toBeVisible();
  });
});
