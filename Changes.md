# StackManager.Proxy Security Improvements

This document summarizes all security improvements made to the StackManager.Proxy project.

## Summary of Changes

### Security Improvements Implemented:
1. **Input Validation** - Comprehensive validation across all API endpoints
2. **Rate Limiting** - Protection against brute force attacks
3. **Error Message Sanitization** - Removed sensitive details from error responses
4. **Security Headers** - Added protection against common web vulnerabilities
5. **CORS Configuration** - Removed unnecessary CORS (CLI access pattern)
6. **Request Size Limits** - Prevention of DoS via large payloads
7. **Hardcoded Credential Removal** - Removed sensitive information from error messages
8. **Regex Optimization** - Created shared regex utility to eliminate code duplication

## Detailed Changes by File

### 📁 `src/StackManager.Proxy/Utilities/RegexPatterns.cs` (NEW FILE)

**Lines Added: 1-70**

**Changes Made:**
- **Created shared regex utility class** with generated regex patterns
- **KubernetesNameRegex()** - Validates Kubernetes DNS-1123 compliant names
- **StorageSizeRegex()** - Validates storage size quantities (e.g., 10Gi, 500M)
- **UrlRegex()** - Validates absolute URLs
- **Public validation methods** - IsValidKubernetesName(), IsValidStorageSize(), IsValidUrl()
- **Eliminated code duplication** - All services now use shared regex patterns

**Security Improvements:**
- ✅ Consistent validation across all services
- ✅ Better performance with generated regex
- ✅ Reduced code duplication and maintenance burden
- ✅ Type-safe regex patterns with compile-time generation

### 📁 `src/StackManager.Proxy/Program.cs`

**Lines Changed: 3, 79-109**

**Changes Made:**
1. **Added Rate Limiting** (Lines 79-88)
   - Added `Microsoft.AspNetCore.RateLimiting` using directive
   - Configured fixed window rate limiter (100 requests/minute)
   - Small queue for burst handling (2 requests)
   - Applied rate limiting middleware

2. **Added Security Headers** (Lines 94-101)
   - `X-Content-Type-Options: nosniff` - Prevents MIME sniffing
   - `X-Frame-Options: DENY` - Prevents clickjacking
   - `X-XSS-Protection: 1; mode=block` - Enables XSS protection
   - `Referrer-Policy: no-referrer` - Prevents referrer leakage
   - `Content-Security-Policy: default-src 'self'` - Restricts resource loading

3. **CORS Configuration** (Lines 103-104)
   - **Removed CORS restrictions** since API is accessed by CLI tools, not browsers
   - CLI clients don't enforce CORS policies, making CORS unnecessary
   - Added explanatory comment for future reference

4. **Added Request Size Limits** (Lines 110-116)
   - 1MB maximum request size limit
   - Returns 413 Payload Too Large for oversized requests
   - Prevents DoS via large payloads

**Security Improvements:**
- ✅ Protection against brute force attacks
- ✅ Prevention of clickjacking and MIME sniffing
- ✅ Reduced XSS attack surface
- ✅ Appropriate configuration for CLI access pattern
- ✅ Prevention of DoS via large requests

### 📁 `src/StackManager.Proxy/Services/ArgoService.cs`

**Lines Changed: Multiple sections throughout the file**

**Changes Made:**

1. **Input Validation for Application Creation** (Lines ~107-120)
   - Added null/empty checks for Name, Repository, Path
   - Added Kubernetes DNS name validation (alphanumeric + hyphens, max 63 chars)
   - Throws `BadRequestError` with descriptive messages

2. **Input Validation for Application Updates** (Lines ~150-160)
   - Added null/empty checks for Name, Repository, Path
   - Consistent validation with creation endpoint

3. **Input Validation for Repository Creation** (Lines ~340-360)
   - Added null/empty checks for Name, Url, Username, Password
   - Added URL format validation using `Uri.TryCreate()`
   - Added Kubernetes DNS name validation
   - Sanitized empty credentials to empty strings

4. **Error Message Sanitization** (Multiple locations)
   - Removed response codes from error messages
   - Removed internal service details
   - Replaced with generic user-friendly messages
   - Added `SanitizeErrorMessage()` helper method

**Security Improvements:**
- ✅ Prevention of invalid input processing
- ✅ Protection against injection attacks
- ✅ Reduced information leakage in errors
- ✅ Consistent validation across endpoints
- ✅ Refactored to use shared RegexPatterns utility

### 📁 `src/StackManager.Proxy/Services/RancherService.cs`

**Lines Changed: ~85-95, ~130-150**

**Changes Made:**

1. **Input Validation for Namespace Creation** (Lines ~85-95)
   - Added null/empty check for namespace name
   - Added Kubernetes DNS name validation
   - Throws `BadRequestError` with descriptive message

2. **Input Validation for PersistentVolumeClaim Creation** (Lines ~130-150)
   - Added null/empty checks for namespace, name, volumeName, accessMode, storageSize
   - Added Kubernetes DNS name validation
   - Added access mode validation (ReadWriteOnce, ReadOnlyMany, ReadWriteMany)
   - Added storage size format validation (e.g., 10Gi, 500M)

**Security Improvements:**
- ✅ Prevention of invalid Kubernetes resource creation
- ✅ Protection against malformed storage requests
- ✅ Consistent validation with Kubernetes requirements

### 📁 `src/StackManager.Proxy/Services/RancherService.PVC.cs`

**Lines Changed: ~60-90**

**Changes Made:**
- **Enhanced Input Validation for PersistentVolumeClaim Creation**
  - Added comprehensive validation matching the main RancherService
  - Added namespace, name, volumeName, accessMode, storageSize validation
  - Added Kubernetes DNS name validation
  - Added access mode and storage size format validation

**Security Improvements:**
- ✅ Consistent validation across all PVC operations
- ✅ Prevention of invalid storage resource creation

### 📁 `src/StackManager.Proxy/Services/RancherService.PV.cs`

**Lines Changed: ~50-80**

**Changes Made:**
- **Enhanced Input Validation for PersistentVolume Creation**
  - Added null/empty checks for name, volumeHandle, accessMode, storageSize
  - Added Kubernetes DNS name validation
  - Added access mode validation
  - Added storage size format validation

**Security Improvements:**
- ✅ Prevention of invalid persistent volume creation
- ✅ Protection against malformed storage requests

### 📁 `src/StackManager.Proxy/Services/LonghornService.cs`

**Lines Changed: ~90-120**

**Changes Made:**
- **Comprehensive Input Validation for Volume Creation**
  - Added null/empty checks for name, size, accessMode, frontend
  - Added Kubernetes DNS name validation
  - Added access mode validation (rwo, rwx, rox)
  - Added frontend validation (must be 'blockdev')
  - Added storage size format validation

**Security Improvements:**
- ✅ Prevention of invalid Longhorn volume creation
- ✅ Protection against malformed storage requests
- ✅ Consistent validation with Longhorn requirements

### 📁 `src/StackManager.Proxy/Controller/VolumeController.cs`

**Lines Changed: ~45-65, ~110-120**

**Changes Made:**

1. **Input Validation for Volume Creation** (Lines ~45-65)
   - Added null/empty checks for namespace and all volume parameters
   - Added `ProducesResponseType` for 400 Bad Request
   - Throws `BadRequestError` with descriptive messages

2. **Input Validation for Volume Deletion** (Lines ~110-120)
   - Added null/empty checks for namespace and volume name
   - Added `ProducesResponseType` for 400 Bad Request

**Security Improvements:**
- ✅ Prevention of invalid volume operations
- ✅ Consistent validation at controller level
- ✅ Proper HTTP status codes for validation failures

## Security Impact Assessment

### 🔒 Vulnerabilities Mitigated:

1. **Brute Force Attacks** ✅
   - Mitigated by rate limiting (100 requests/minute)

2. **Information Leakage** ✅
   - Mitigated by sanitized error messages
   - Removed response codes, URLs, and internal details

3. **Injection Attacks** ✅
   - Mitigated by comprehensive input validation
   - All user inputs validated before processing

4. **Cross-Site Scripting (XSS)** ✅
   - Mitigated by security headers (X-XSS-Protection, CSP)

5. **Clickjacking** ✅
   - Mitigated by X-Frame-Options header

6. **MIME Sniffing** ✅
   - Mitigated by X-Content-Type-Options header

7. **Cross-Origin Attacks** ❌
   - Not applicable (CLI clients don't enforce CORS)
   - CORS restrictions removed as unnecessary

8. **Denial of Service (DoS)** ✅
   - Mitigated by request size limits (1MB max)
   - Mitigated by rate limiting

### 📊 Security Score Improvement:

**Before:**
- Input Validation: ❌ (No comprehensive validation)
- Rate Limiting: ❌ (None)
- Error Handling: ❌ (Exposed internal details)
- Security Headers: ❌ (None)
- CORS Protection: ❌ (Wide open)
- DoS Protection: ❌ (No size limits)

**After:**
- Input Validation: ✅ (Comprehensive validation)
- Rate Limiting: ✅ (100 requests/minute)
- Error Handling: ✅ (Sanitized messages)
- Security Headers: ✅ (All major headers)
- CORS Protection: ❌ (Not applicable for CLI access)
- DoS Protection: ✅ (1MB size limit)

## Testing & Verification

**Build Status:** ✅ Successful
**Tests Status:** ✅ Passing
**Security Improvements:** ✅ All implemented and working

## Recommendations for Future Enhancements

1. **Audit Logging** - Implement logging for sensitive operations
2. **RBAC** - Add role-based access control for fine-grained permissions
3. **Token Rotation** - Implement API token expiration and rotation
4. **Request Validation** - Add OpenAPI/Swagger validation
5. **Dependency Scanning** - Implement regular vulnerability scanning

## Conclusion

These changes significantly improve the security posture of the StackManager.Proxy API by addressing critical vulnerabilities while maintaining all existing functionality. The service is now much more resilient against common web attacks and information leakage.