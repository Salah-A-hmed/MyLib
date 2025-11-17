window.onload = function() {
    const loader = document.getElementById('loader');
    
    document.body.style.overflow = 'hidden';
    
    setTimeout(() => {
        loader.style.transition = 'opacity 0.5s ease-out';
        loader.style.opacity = '0';
        
        setTimeout(() => {
            loader.style.display = 'none';
            document.body.style.overflow = 'auto';
            
            // Trigger hero animations manually after loader
            triggerHeroAnimations();
            
            // Initialize WOW for other sections below hero
            new WOW({
                boxClass: 'wow',
                animateClass: 'animate__animated',
                offset: 100,
                mobile: true,
                live: true
            }).init();
        }, 500);
    }, 1200);
};

// Function to trigger hero section animations with CSS transitions
function triggerHeroAnimations() {
    const heroTitle = document.querySelector('.hero-title');
    const heroText = document.querySelector('.hero-text');
    const heroButton = document.querySelector('.hero-button');
    
    // Animate title
    setTimeout(() => {
        heroTitle.style.opacity = '1';
        heroTitle.style.transform = 'translateY(0)';
    }, 100);
    
    // Animate text
    setTimeout(() => {
        heroText.style.opacity = '1';
        heroText.style.transform = 'translateY(0)';
    }, 500);
    
    // Animate button
    setTimeout(() => {
        heroButton.style.opacity = '1';
        heroButton.style.transform = 'translateY(0)';
    }, 900);
}


// Initialize Swiper for Testimonials
const testimonialSwiper = new Swiper('.testimonialSwiper', {
    slidesPerView: 1,
    spaceBetween: 30,
    loop: true,
    autoplay: {
        delay: 5000,
        disableOnInteraction: false,
    },
    pagination: {
        el: '.swiper-pagination',
        clickable: true,
    },
    // Enable touch/mouse drag
    simulateTouch: true,
    allowTouchMove: true,
    effect: 'slide', // Changed from fade to slide for better drag feeling
    speed: 600,
});