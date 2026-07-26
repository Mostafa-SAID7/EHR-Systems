import { FormControl, FormGroup } from '@angular/forms';
import { passwordStrengthValidator, PasswordStrength, matchPasswordValidator } from './password-strength.validator';

describe('Password Validators', () => {
  describe('passwordStrengthValidator', () => {
    let control: FormControl;

    beforeEach(() => {
      control = new FormControl('');
    });

    it('should return null for empty password', () => {
      control.setValue('');
      const result = passwordStrengthValidator()(control);
      expect(result).toBeNull();
    });

    it('should reject weak password', () => {
      control.setValue('weak');
      const result = passwordStrengthValidator(PasswordStrength.Strong)(control);
      expect(result).not.toBeNull();
      expect(result?.['passwordStrength']).toBeDefined();
    });

    it('should accept strong password', () => {
      control.setValue('StrongPass123!@');
      const result = passwordStrengthValidator(PasswordStrength.Strong)(control);
      expect(result).toBeNull();
    });

    it('should accept very strong password', () => {
      control.setValue('VeryStrongPassword123!@#$%^&*');
      const result = passwordStrengthValidator(PasswordStrength.VeryStrong)(control);
      expect(result).toBeNull();
    });

    it('should check length', () => {
      control.setValue('abc');
      const result = passwordStrengthValidator(PasswordStrength.Strong)(control);
      expect(result).not.toBeNull();
    });

    it('should check uppercase letters', () => {
      control.setValue('abcdefgh123!@');
      const result = passwordStrengthValidator(PasswordStrength.Strong)(control);
      expect(result).not.toBeNull();
    });

    it('should check special characters', () => {
      control.setValue('AbcDefgh123');
      const result = passwordStrengthValidator(PasswordStrength.Strong)(control);
      expect(result).not.toBeNull();
    });
  });

  describe('matchPasswordValidator', () => {
    it('should validate matching passwords', () => {
      const form = new FormGroup({
        password: new FormControl('Test1234!@'),
        confirmPassword: new FormControl('Test1234!@'),
      });

      const control = form.get('confirmPassword') as FormControl;
      const result = matchPasswordValidator('password')(control);
      expect(result).toBeNull();
    });

    it('should reject non-matching passwords', () => {
      const form = new FormGroup({
        password: new FormControl('Test1234!@'),
        confirmPassword: new FormControl('Different1234!@'),
      });

      const control = form.get('confirmPassword') as FormControl;
      const result = matchPasswordValidator('password')(control);
      expect(result).not.toBeNull();
      expect(result?.['passwordMismatch']).toBe(true);
    });

    it('should return null for empty password', () => {
      const control = new FormControl('');
      const result = matchPasswordValidator('password')(control);
      expect(result).toBeNull();
    });
  });
});
