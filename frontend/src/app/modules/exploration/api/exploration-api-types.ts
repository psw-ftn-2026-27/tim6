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
