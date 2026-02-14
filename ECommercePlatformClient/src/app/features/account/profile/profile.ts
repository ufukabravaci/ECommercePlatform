import { Component, effect, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProfileService } from '../../../core/services/profile-service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile.html',
  styleUrls: ['./profile.scss']
})
export class ProfileComponent implements OnInit {
  private readonly profileService = inject(ProfileService);
  private readonly fb = inject(FormBuilder);

  profileForm!: FormGroup;

  // Servis Sinyalleri (Template'de kullanmak için)
  loading = this.profileService.loading;
  error = this.profileService.error;
  profile = this.profileService.profile;
  
  // Local State
  submitting = signal<boolean>(false);
  successMessage = signal<string | null>(null);

  ngOnInit() {
    this.initForm();
    this.loadProfile();
  }

  private initForm() {
    this.profileForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: [{ value: '', disabled: true }],
      phoneNumber: [''],
      address: this.fb.group({
        city: ['', Validators.required],
        district: ['', Validators.required],
        street: ['', Validators.required],
        zipCode: [''],
        fullAddress: ['', Validators.required]
      })
    });
  }

  loadProfile() {
    // API isteğini başlat
    this.profileService.loadMyProfile().subscribe({
      next: (res) => {
        // Veri geldiğinde formu doldur (Effect yerine burada yapıyoruz)
        if (res.isSuccessful && res.data) {
          this.updateForm(res.data);
        }
      }
    });
  }

  private updateForm(data: any) {
    this.profileForm.patchValue({
      firstName: data.firstName,
      lastName: data.lastName,
      email: data.email,
      phoneNumber: data.phoneNumber,
      address: data.address || { city: '', district: '', street: '', zipCode: '', fullAddress: '' }
    });
  }

  onSubmit() {
    if (this.profileForm.invalid) {
      this.markFormGroupTouched(this.profileForm);
      return;
    }

    this.submitting.set(true);
    this.successMessage.set(null);
    this.profileService.clearError();

    const payload = this.profileForm.getRawValue();
    const updateData = {
        firstName: payload.firstName,
        lastName: payload.lastName,
        phoneNumber: payload.phoneNumber,
        address: payload.address
    };

    this.profileService.updateProfile(updateData).subscribe({
      next: (res) => {
        if (res.isSuccessful) {
          this.successMessage.set('Profil başarıyla güncellendi.');
          setTimeout(() => this.successMessage.set(null), 3000);
        }
        this.submitting.set(false);
      },
      error: () => this.submitting.set(false)
    });
  }

  private markFormGroupTouched(formGroup: FormGroup) {
    Object.values(formGroup.controls).forEach(control => {
      control.markAsTouched();
      if ((control as any).controls) {
        this.markFormGroupTouched(control as any);
      }
    });
  }
}