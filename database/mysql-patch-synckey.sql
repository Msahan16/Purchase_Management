-- Optional / unused by current API: the EF model does not map SyncKey (works with original DDL).
-- You can run this if you later add server-side idempotency again.
USE purchase_management_db;

ALTER TABLE PurchaseBills ADD COLUMN SyncKey VARCHAR(36) NULL;

CREATE UNIQUE INDEX IX_PurchaseBills_SyncKey ON PurchaseBills (SyncKey);
