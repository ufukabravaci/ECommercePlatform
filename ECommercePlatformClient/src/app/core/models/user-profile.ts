export interface AddressDto {
  city: string;
  district: string;
  street: string;
  zipCode: string;
  fullAddress: string;
}

export interface UserProfile {
  firstName: string;
  lastName: string;
  email: string;
  userName: string;
  phoneNumber?: string;
  address?: AddressDto;
}