export type TourDifficulty = 'Easy' | 'Moderate' | 'Hard';

export type TourStatus = 'Draft' | 'Published' | 'Archived';

export type TransportMode = 'Walking' | 'Bicycle' | 'Car';

export interface TransportTimeDto {
  transport: TransportMode;
  minutes: number;
}

export interface RecommendedGroupSizeDto {
  min: number;
  max: number;
}

export interface TourDto {
  id: string;
  authorId: string;
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  status: TourStatus;
  publishedAt: string | null;
  archivedAt: string | null;
  transportTimes: TransportTimeDto[];
  recommendedGroupSize: RecommendedGroupSizeDto;
}

export interface CreateTourDto {
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  recommendedGroupSize: RecommendedGroupSizeDto;
}

export interface TourPreferenceDto {
  difficulties: TourDifficulty[];
  transport: TransportMode;
  tags: string[];
  applyToSearch: boolean;
}

export interface TourPriceDto {
  isFree: boolean;
  amount: number | null;
}

export interface PublishTourDto {
  price: TourPriceDto | null;
}

export interface LocationDto {
  latitude: number;
  longitude: number;
}

export interface KeyPointDto {
  id: string;
  order: number;
  name: string;
  description: string;
  image: string;
  secret: string | null;
  location: LocationDto;
  isVisibleWithoutPurchase: boolean;
}

export interface TourKeyPointsDto {
  status: TourStatus;
  keyPoints: KeyPointDto[];
  lengthInKilometers: number;
}

export interface SaveKeyPointDto {
  name: string;
  description: string;
  image: string;
  secret: string | null;
  location: LocationDto;
  isVisibleWithoutPurchase: boolean;
  lengthInKilometers: number;
}

export interface RemoveKeyPointDto {
  lengthInKilometers: number;
}

export interface KeyPointPreviewDto {
  id: string;
  name: string;
  description: string;
  image: string;
  location: LocationDto;
}

export interface TourPreviewDto {
  id: string;
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  recommendedGroupSize: RecommendedGroupSizeDto;
  lengthInKilometers: number;
  transportTimes: TransportTimeDto[];
  keyPoints: KeyPointPreviewDto[];
}

export interface NextKeyPointDto {
  name: string;
  description: string;
  image: string;
  location: LocationDto;
}

export type TourExecutionStatus = 'Active' | 'Completed' | 'Abandoned';

export interface UnlockedKeyPointDto {
  name: string;
  location: LocationDto;
  secret: string | null;
}

export interface TourExecutionDto {
  id: string;
  tourId: string;
  status: TourExecutionStatus;
  nextKeyPoint: NextKeyPointDto | null;
  lastReachedKeyPoint: UnlockedKeyPointDto | null;
}
