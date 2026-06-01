import { useState } from "react";
import { useListingPhotos } from "../../hooks/useListingPhotos";
import "./ListingGallery.css";

const ListingGallery = ({ listingId }: { listingId: string }) => {
  const { photos, isLoading } = useListingPhotos(listingId);
  const [activeIndex, setActiveIndex] = useState(0);

  if (isLoading) {
    return <div className="listing-detail-hero skeleton">Ładowanie...</div>;
  }

  if (!photos || photos.length === 0 || !photos[0].url) {
    return null;
  }

  const next = () => setActiveIndex((prev) => (prev + 1) % photos.length);
  const prev = () => setActiveIndex((prev) => (prev - 1 + photos.length) % photos.length);

  console.log("photos", photos);

  return (
    <div className="listing-detail-hero">
      <img src={photos[activeIndex].url} alt="Listing" />
      
      {photos.length > 1 && (
        <>
          <button className="nav-btn prev" onClick={prev}>❮</button>
          <button className="nav-btn next" onClick={next}>❯</button>
          <div className="counter">
            {activeIndex + 1} / {photos.length}
          </div>
        </>
      )}
    </div>
  );
};

export default ListingGallery;