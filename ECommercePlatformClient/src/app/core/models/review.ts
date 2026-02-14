export interface ReviewDto {
  id: string;
  customerId: string;
  customerName: string;
  rating: number; // 1-5
  comment: string;
  createdAt: string; // DateTimeOffset -> string
  sellerReply?: string;
  sellerRepliedAt?: string;
}

export interface ReviewDetailDto extends ReviewDto {
  productId: string;
  productName: string;
  isApproved: boolean;
}

export interface CreateReviewRequest {
  productId: string;
  rating: number;
  comment: string;
}