-- =============================================================================
-- Migration: Create security.trusted_devices table (H-23 / H-24)
-- Description: Persists trusted devices issued during 2FA RememberDevice flow
-- to allow instant, reliable revocation upon password reset, session revocation,
-- or 2FA deactivation.
-- =============================================================================

CREATE TABLE IF NOT EXISTS security.trusted_devices (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    device_name VARCHAR(100),
    is_revoked BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    revoked_at TIMESTAMPTZ NULL,
    CONSTRAINT fk_trusted_devices_user FOREIGN KEY (user_id) REFERENCES security.users(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_trusted_devices_user_active ON security.trusted_devices (user_id, is_revoked);
