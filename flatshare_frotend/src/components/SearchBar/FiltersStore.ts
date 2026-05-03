import { create } from "zustand";
import { locationConfig } from "./locationConfig";
import { profileConfig } from "./profileConfig";

export type City = keyof typeof locationConfig;

export type Profile = keyof typeof profileConfig;
export type ProfileValue = Profile | "";

export interface Filters {
  page: number;
  size: number;
  city: City | "";
  district: string;
  minPrice: string;
  maxPrice: string;
  petsAllowed: boolean;
  nonSmokingOnly: boolean;
  closeToShops: string;
  profile: ProfileValue;
  minArea: string;
  maxArea: string;
  startDate: string;
}

interface FiltersState {
  filters: Filters;
  setFilter: <K extends keyof Filters>(key: K, value: Filters[K]) => void;
  resetFilters: () => void;
}

const initialFilters: Filters = {
  page: 0,
  size: 10,
  city: "",
  district: "",
  minPrice: "",
  maxPrice: "",
  petsAllowed: false,
  nonSmokingOnly: false,
  closeToShops: "",
  profile: "",
  minArea: "",
  maxArea: "",
  startDate: ""
};

export const useFiltersStore = create<FiltersState>((set) => ({
  filters: initialFilters,

  setFilter: (key, value) =>
    set((state) => ({
      filters: {
        ...state.filters,
        [key]: value
      }
    })),

  resetFilters: () => set({ filters: initialFilters })
}));