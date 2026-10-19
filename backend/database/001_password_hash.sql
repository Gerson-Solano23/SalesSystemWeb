-- Widens Usuario.password so it can store PBKDF2 hashes (~85 characters).
-- Run once against the MySalesCompany database BEFORE deploying the new API version.
--
-- Existing plain-text passwords keep working: the API re-hashes each one
-- automatically the first time that user logs in successfully.

USE MySalesCompany;
GO

ALTER TABLE dbo.Usuario ALTER COLUMN password VARCHAR(256) NULL;
GO

-- Optional: list users whose password is still stored in plain text.
-- SELECT idUser, email FROM dbo.Usuario WHERE password NOT LIKE 'PBKDF2$%';
