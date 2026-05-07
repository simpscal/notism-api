# Migrate Pending Banking Orders to COD

Existing orders with `PaymentMethod=banking` and `PaymentStatus=unpaid` are migrated to `cashOnDelivery` so they can proceed to fulfillment under the new payment flow.

> The EF Core migration (`MigrateBankingOrdersToCod`) runs automatically on deploy. This runbook is for BA verification before and after.

---

## Pre-migration backup

```sql
SELECT * INTO orders_backup_2026_05_07
FROM "Orders"
WHERE "PaymentMethod" = 'banking' AND "PaymentStatus" = 'unpaid';
```

## Migration

```sql
UPDATE "Orders"
SET "PaymentMethod" = 'cashOnDelivery'
WHERE "PaymentMethod" = 'banking' AND "PaymentStatus" = 'unpaid';
```

## Verification

```sql
SELECT COUNT(*) FROM "Orders"
WHERE "PaymentMethod" = 'banking' AND "PaymentStatus" = 'unpaid';
-- Expected: 0
```

## Rollback

```sql
UPDATE "Orders"
SET "PaymentMethod" = 'banking'
WHERE "PaymentMethod" = 'cashOnDelivery' AND "PaymentStatus" = 'unpaid';
```
