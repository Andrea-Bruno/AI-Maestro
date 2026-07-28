# Profile Signatures

Cryptographically sign your roast profiles for integrity verification.

## Key Generation

Click **Generate Keys** to create a new ECDSA P-256 key pair:
- **Private key**: Keep secret — used for signing
- **Public key**: Share publicly — used for verification

## Signing a Profile

1. Select the profile
2. Enter your **private key** (hex format)
3. Click **Sign**
4. The signature is stored in the profile metadata

## Verifying a Profile

1. Select the signed profile
2. Enter the **public key** (hex format)
3. Click **Verify**
4. Result: "✅ Valid signature" or "❌ Invalid"

## Use Cases

- **Traceability**: Prove a roast profile hasn't been tampered with
- **Authentication**: Verify the roaster who created the profile
- **Compliance**: Meet documentation requirements for specialty coffee buyers
